using MainCore.Commands.UI.Misc;
using MainCore.Tasks.Base;
using Polly;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class TrainTroopTask(
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeBrowser browser,
        ToDorfCommand.Handler toDorfCommand,
        UpdateBuildingCommand.Handler updateBuildingCommand,
        ToBuildingByTypeCommand.Handler toBuildingCommand,
        SaveVillageSettingCommand.Handler saveVillageSettingCommand,
        ILogger logger)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => "Train troop";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.TrainTroopEnable);
                if (!settingEnable) return false;
                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            Result result;
            var buildings = GetTrainTroopBuildings(task.VillageId);
            var settings = new Dictionary<VillageSettingEnums, int>();

            foreach (var building in buildings)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                result = await ToTrainTroopPage(task.VillageId, building, cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<MissingBuilding>())
                    {
                        logger.Warning("Disable train troop on {Building} because the building is missing.", building);
                        settings.Add(TroopSettings[building], 0);
                        continue;
                    }

                    await saveVillageSettingCommand.HandleAsync(new(task.AccountId, task.VillageId, settings), cancellationToken);
                    return result;
                }

                result = await TrainTroop(task.VillageId, building);
                if (result.IsFailed)
                {
                    if (result.HasError<MissingResource>())
                    {
                        break;
                    }

                    await saveVillageSettingCommand.HandleAsync(new(task.AccountId, task.VillageId, settings), cancellationToken);
                    return result;
                }
            }

            await saveVillageSettingCommand.HandleAsync(new(task.AccountId, task.VillageId, settings), cancellationToken);
            task.ExecuteAt = GetNextExecute(task.VillageId);
            return Result.Ok();
        }

        private async ValueTask<Result> ToTrainTroopPage(VillageId villageId, BuildingEnums building, CancellationToken cancellationToken)
        {
            var result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            result = await toBuildingCommand.HandleAsync(new(villageId, building), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private List<BuildingEnums> GetTrainTroopBuildings(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var settings = context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => TroopSettings.Values.Contains(x.Setting))
                .Where(x => x.Value != 0)
                .Select(x => x.Setting)
                .ToList();

            var buildings = new List<BuildingEnums>();

            if (settings.Contains(VillageSettingEnums.BarrackTroop))
            {
                buildings.Add(BuildingEnums.Barracks);
            }
            if (settings.Contains(VillageSettingEnums.StableTroop))
            {
                buildings.Add(BuildingEnums.Stable);
            }
            if (settings.Contains(VillageSettingEnums.GreatBarrackTroop))
            {
                buildings.Add(BuildingEnums.GreatBarracks);
            }
            if (settings.Contains(VillageSettingEnums.GreatStableTroop))
            {
                buildings.Add(BuildingEnums.GreatStable);
            }
            if (settings.Contains(VillageSettingEnums.WorkshopTroop))
            {
                buildings.Add(BuildingEnums.Workshop);
            }

            return buildings;
        }

        private DateTime GetNextExecute(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var seconds = context.ByName(
                villageId,
                VillageSettingEnums.TrainTroopRepeatTimeMin,
                VillageSettingEnums.TrainTroopRepeatTimeMax,
                60
            );
            var nextExecute = DateTime.Now.AddSeconds(seconds);
            return nextExecute;
        }

        private static Dictionary<BuildingEnums, VillageSettingEnums> TroopSettings { get; } = new()
        {
            {BuildingEnums.Barracks, VillageSettingEnums.BarrackTroop },
            {BuildingEnums.Stable, VillageSettingEnums.StableTroop },
            {BuildingEnums.GreatBarracks, VillageSettingEnums.GreatBarrackTroop },
            {BuildingEnums.GreatStable, VillageSettingEnums.GreatStableTroop },
            {BuildingEnums.Workshop, VillageSettingEnums.WorkshopTroop },
        };

        private static Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> AmountSettings { get; } = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax ) },
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.GreatBarracks, (VillageSettingEnums.GreatBarrackAmountMin,VillageSettingEnums.GreatBarrackAmountMax ) },
            {BuildingEnums.GreatStable, (VillageSettingEnums.GreatStableAmountMin,VillageSettingEnums.GreatStableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        private async ValueTask<Result> Train(TroopEnums troop, long amount)
        {
            Result result;
            result = await browser.Input(TrainTroopParser.GetInputBox(browser.CurrentPage, troop), $"{amount}");
            if (result.IsFailed) return result;

            result = await browser.Click(TrainTroopParser.GetTrainButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private (TroopEnums troop, int amount) GetTroopDataToTrain(VillageId villageId, BuildingEnums building)
        {
            using var context = contextFactory.CreateDbContext();
            var troop = (TroopEnums)context.ByName(villageId, TroopSettings[building]);
            var (minSetting, maxSetting) = AmountSettings[building];
            var amount = context.ByName(villageId, minSetting, maxSetting);
            return (troop, amount);
        }

        private bool TrainWhenLowResource(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            return context.BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
        }

        private async ValueTask<Result> TrainTroop(VillageId villageId, BuildingEnums building)
        {
            var (troop, amount) = GetTroopDataToTrain(villageId, building);
            var maxAmount = await TrainTroopParser.GetMaxAmount(browser.CurrentPage, troop);
            if (maxAmount == 0)
            {
                return MissingResource.Error(troop);
            }

            if (amount > maxAmount && !TrainWhenLowResource(villageId))
            {
                return MissingResource.Error(troop);
            }

            var result = await Train(troop, amount);
            if (result.IsFailed) return result;

            logger.Information("Troop training for {Troop} with amount {Amount} is done.", troop, amount);
            return Result.Ok();
        }
    }
}