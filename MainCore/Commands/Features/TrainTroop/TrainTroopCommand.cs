using MainCore.Errors.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class TrainTroopCommand
    {
        public sealed record Command(VillageId VillageId, BuildingEnums Building) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ISettingService settingService,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            CancellationToken cancellationToken)
        {
            var (villageId, building) = command;

            Result result;
            result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, response, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var buildingLocation = response.Buildings
                .Where(x => x.Type == building)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (buildingLocation == default)
            {
                return MissingBuilding.Error(building);
            }

            result = await toBuildingCommand.HandleAsync(new(buildingLocation), cancellationToken);
            if (result.IsFailed) return result;

            var troopSetting = BuildingSettings[building];
            var troop = (TroopEnums)settingService.ByName(villageId, troopSetting);

            (_, isFailed, var amount, errors) = GetAmount(settingService, browser, villageId, building, troop);
            if (isFailed) return Result.Fail(errors);

            result = await TrainTroop(browser, troop, amount);
            if (result.IsFailed) return result;

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

        public static Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> AmountSettings { get; } = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax ) },
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.GreatBarracks, (VillageSettingEnums.GreatBarrackAmountMin,VillageSettingEnums.GreatBarrackAmountMax ) },
            {BuildingEnums.GreatStable, (VillageSettingEnums.GreatStableAmountMin,VillageSettingEnums.GreatStableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        private static Result<long> GetAmount(
            ISettingService settingService,
            IChromeBrowser browser,
            VillageId villageId,
            BuildingEnums building,
            TroopEnums troop)
        {
            var maxAmount = TrainTroopParser.GetMaxAmount(browser.Html, troop);

            if (maxAmount == 0)
            {
                return MissingResource.Error(building);
            }

            var (minSetting, maxSetting) = AmountSettings[building];
            var amount = settingService.ByName(villageId, minSetting, maxSetting);
            if (amount < maxAmount)
            {
                return amount;
            }

            var trainWhenLowResource = settingService.BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
            if (!trainWhenLowResource)
            {
                return MissingResource.Error(building);
            }

            amount = maxAmount;
            return amount;
        }

        private static async ValueTask<Result> TrainTroop(
            IChromeBrowser browser,
            TroopEnums troop,
            long amount)
        {
            var inputBox = TrainTroopParser.GetInputBox(browser.Html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            Result result;
            result = await browser.Input(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result;

            var trainButton = TrainTroopParser.GetTrainButton(browser.Html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await browser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}