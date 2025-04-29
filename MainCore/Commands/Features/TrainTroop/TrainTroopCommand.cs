using MainCore.Common.Errors.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class TrainTroopCommand
    {
        public sealed record Command(AccountId AccountId, BuildingEnums Building) : ICustomCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            IGetSetting getSetting,
            GetBuildingLocationQuery.Handler getBuildingLocation,
            CancellationToken cancellationToken)
        {
            var (accountId, building) = command;

            var result = await ToTrainBuilding(accountId, building, chromeManager, toDorfCommand, updateBuildingCommand, toBuildingCommand, getBuildingLocation, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var troopSetting = BuildingSettings[building];
            var troop = (TroopEnums)getSetting.ByName(chromeManager.Get(accountId).VillageId, troopSetting);

            var (_, isFailed, amount, errors) = GetAmount(building, troop, chromeManager, getSetting);
            if (isFailed) return Result.Fail(errors).WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await TrainTroop(troop, amount, chromeManager, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        public static Dictionary<BuildingEnums, VillageSettingEnums> BuildingSettings { get; } = new()
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

        private static async ValueTask<Result> ToTrainBuilding(
            AccountId accountId,
            BuildingEnums building,
            IChromeManager chromeManager,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            GetBuildingLocationQuery.Handler getBuildingLocation,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toDorfCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await updateBuildingCommand.HandleAsync(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var buildingLocation = await getBuildingLocation.HandleAsync(new(chromeManager.Get(accountId).VillageId, building), cancellationToken);
            if (buildingLocation == default)
            {
                return MissingBuilding.Error(building);
            }

            result = await toBuildingCommand.HandleAsync(new(accountId, buildingLocation), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static Result<long> GetAmount(
            BuildingEnums building,
            TroopEnums troop,
            IChromeManager chromeManager,
            IGetSetting getSetting)
        {
            var villageId = chromeManager.GetVillageId();
            var (minSetting, maxSetting) = AmountSettings[building];
            var amount = getSetting.ByName(villageId, minSetting, maxSetting);

            var html = chromeManager.GetHtml();

            var maxAmount = TrainTroopParser.GetMaxAmount(html, troop);

            if (maxAmount == 0)
            {
                return MissingResource.Error(building);
            }

            if (amount < maxAmount)
            {
                return amount;
            }

            var trainWhenLowResource = getSetting.BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
            if (!trainWhenLowResource)
            {
                return MissingResource.Error(building);
            }

            amount = maxAmount;
            return amount;
        }

        private static async ValueTask<Result> TrainTroop(
            TroopEnums troop,
            long amount,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            var html = chromeManager.GetHtml();

            var inputBox = TrainTroopParser.GetInputBox(html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            Result result;
            result = await chromeManager.Input(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var trainButton = TrainTroopParser.GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await chromeManager.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}