using MainCore.Commands.UI.Misc;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class NpcTask(
        SaveVillageSettingCommand.Handler saveVillageSettingCommand,
        ToDorfCommand.Handler toDorfCommand,
        UpdateBuildingCommand.Handler updateBuildingCommand,
        ToBuildingByTypeCommand.Handler toBuildingCommand,
        SwitchTabCommand.Handler switchTabCommand,
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeBrowser browser,
        ILogger logger,
        ITaskManager taskManager)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => "NPC";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoNPCEnable);
                if (!settingEnable) return false;

                var gold = context.AccountsInfo
                    .Where(x => x.AccountId == AccountId.Value)
                    .Select(x => x.Gold)
                    .FirstOrDefault();
                if (gold < 3) return false;

                var granaryPercent = (int)context.Storages
                   .Where(x => x.VillageId == VillageId.Value)
                   .Select(x => x.Crop * 100f / x.Granary)
                   .FirstOrDefault();

                var autoNPCGranaryPercent = context.ByName(VillageId, VillageSettingEnums.AutoNPCGranaryPercent);
                if (granaryPercent < autoNPCGranaryPercent) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task)
        {
            Result result;
            result = await ToNpcResourcePage(task.AccountId, task.VillageId);
            if (result.IsFailed) return result;

            if (!await CanNPC(task.VillageId))
            {
                return Result.Ok();
            }

            result = await NPCResource(task.AccountId, task.VillageId);
            if (result.IsFailed) return result;

            taskManager.AddOrUpdate(new UpgradeBuildingTask.Task(task.AccountId, task.VillageId));

            return Result.Ok();
        }

        private async ValueTask<Result> ToNpcResourcePage(AccountId accountId, VillageId villageId)
        {
            var result = await toDorfCommand.HandleAsync(new(2));
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(villageId));
            if (result.IsFailed) return result;

            result = await toBuildingCommand.HandleAsync(new(villageId, BuildingEnums.Marketplace));
            if (result.IsFailed)
            {
                if (result.HasError<MissingBuilding>())
                {
                    await TurnOffNPC(accountId, villageId);
                    return Skip.Error.WithErrors(result.Errors);
                }
                return result;
            }
            result = await switchTabCommand.HandleAsync(new(0));
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private async ValueTask TurnOffNPC(AccountId accountId, VillageId villageId)
        {
            var settings = new Dictionary<VillageSettingEnums, int>() {
                        { VillageSettingEnums.AutoNPCEnable, 0 }
                    };
            await saveVillageSettingCommand.HandleAsync(new(accountId, villageId, settings));
            logger.Warning("Disable NPC for this village.");
        }

        private static readonly List<VillageSettingEnums> SettingNames =
        [
            VillageSettingEnums.AutoNPCWood,
            VillageSettingEnums.AutoNPCClay,
            VillageSettingEnums.AutoNPCIron,
            VillageSettingEnums.AutoNPCCrop,
        ];

        private async ValueTask<Result> NPCResource(AccountId accountId, VillageId villageId)
        {
            var result = await OpenDialog();
            if (result.IsFailed) return result;

            var values = await GetValues(villageId);

            result = await CheckOverflow(villageId, values);
            if (result.IsFailed)
            {
                await TurnOffNPC(accountId, villageId);
                return Skip.Error.WithErrors(result.Errors);
            }

            result = await InputAmount(values);
            if (result.IsFailed) return result;

            logger.Information("Current resource:");
            await LogResource();

            result = await browser.Click(NpcResourceParser.GetDistributeButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(NpcResourceParser.GetRedeemButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            logger.Information("After NPC:");
            await LogResource();

            return Result.Ok();
        }

        private async ValueTask<Result> CheckOverflow(VillageId villageId, long[] values)
        {
            using var context = contextFactory.CreateDbContext();
            var overflowNPC = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCOverflow);
            if (overflowNPC) return Result.Ok();

            var warehouse = await StorageParser.GetWarehouseCapacity(browser.CurrentPage);
            for (var i = 0; i < 3; i++)
            {
                if (values[i] > warehouse)
                {
                    return StorageLimit.Warehouse(warehouse, values[i]);
                }
            }

            return Result.Ok();
        }

        private async ValueTask LogResource()
        {
            var wood = await StorageParser.GetWood(browser.CurrentPage);
            var clay = await StorageParser.GetClay(browser.CurrentPage);
            var iron = await StorageParser.GetIron(browser.CurrentPage);
            var crop = await StorageParser.GetCrop(browser.CurrentPage);

            var warehouse = await StorageParser.GetWarehouseCapacity(browser.CurrentPage);
            var granary = await StorageParser.GetGranaryCapacity(browser.CurrentPage);

            logger.Information("[{Warehouse}]: {Wood} - {Clay} - {Iron} | [{Granary}]: {Crop}", warehouse, wood, clay, iron, granary, crop);
        }

        private async ValueTask<bool> CanNPC(VillageId villageId)
        {
            var crop = await StorageParser.GetCrop(browser.CurrentPage);
            var granary = await StorageParser.GetGranaryCapacity(browser.CurrentPage);

            var granaryPercent = (int)(crop * 100f / granary);

            using var context = contextFactory.CreateDbContext();
            var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

            if (granaryPercent < autoNPCGranaryPercent)
            {
                logger.Information("NPC resources not available. Granary percent is too low: {GranaryPercent} < {AutoNPCGranaryPercent}",
                    granaryPercent, autoNPCGranaryPercent);
                return false;
            }

            return true;
        }

        private async ValueTask<Result> OpenDialog()
        {
            var result = await browser.Click(NpcResourceParser.GetExchangeResourcesButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(NpcResourceParser.NpcDialog(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask<Result> InputAmount(long[] values)
        {
            var inputs = NpcResourceParser.GetInputs(browser.CurrentPage);

            var inputCount = await inputs.CountAsync();

            if (inputCount != 4)
            {
                return Stop.Error.WithError($"Expected 4 input elements, but found {inputCount}.");
            }

            for (var i = 0; i < 4; i++)
            {
                var inputElement = inputs.Nth(i);
                var result = await browser.Input(inputElement, $"{values[i]}");
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private async ValueTask<long[]> GetValues(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var settings = context.ByName(villageId, SettingNames);
            var ratio = GetRatio(settings);

            var sum = await NpcResourceParser.GetSum(browser.CurrentPage);
            var sumRatio = ratio.Sum();
            var values = new long[4];
            for (var i = 0; i < 4; i++)
            {
                values[i] = sum * ratio[i] / sumRatio;
            }
            var sumValue = values.Sum();
            var diff = sum - sumValue;
            values[3] += diff;
            return values;
        }

        private static long[] GetRatio(Dictionary<VillageSettingEnums, int> settings)
        {
            var ratio = new long[4]
            {
                settings[VillageSettingEnums.AutoNPCWood],
                settings[VillageSettingEnums.AutoNPCClay],
                settings[VillageSettingEnums.AutoNPCIron],
                settings[VillageSettingEnums.AutoNPCCrop],
            };
            var sum = ratio.Sum();
            if (sum == 0)
            {
                ratio = [.. Enumerable.Repeat<long>(1, 4)];
            }

            return ratio;
        }
    }
}