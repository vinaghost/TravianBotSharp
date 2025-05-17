using MainCore.Constraints;
using MainCore.Errors.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class TrainTroopCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, BuildingEnums Building) : IAccountVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ISettingService settingService,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            GetBuildingLocationQuery.Handler getBuildingLocation,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, building) = command;

            Result result;
            result = await toDorfCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result;

            var updateBuildingCommandResult = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (updateBuildingCommandResult.IsFailed) return Result.Fail(updateBuildingCommandResult.Errors);

            var buildingLocation = await getBuildingLocation.HandleAsync(new(villageId, building), cancellationToken);
            if (buildingLocation == default)
            {
                return MissingBuilding.Error(building);
            }

            result = await toBuildingCommand.HandleAsync(new(accountId, buildingLocation), cancellationToken);
            if (result.IsFailed) return result;

            var troopSetting = BuildingSettings[building];
            var troop = (TroopEnums)settingService.ByName(villageId, troopSetting);

            var (_, isFailed, amount, errors) = GetAmount(settingService, browser, villageId, building, troop);
            if (isFailed) return Result.Fail(errors);

            result = await TrainTroop(browser, troop, amount, cancellationToken);
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
            var html = browser.Html;

            var maxAmount = TrainTroopParser.GetMaxAmount(html, troop);

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
            long amount,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;

            var inputBox = TrainTroopParser.GetInputBox(html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            Result result;
            result = await browser.Input(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result;

            var trainButton = TrainTroopParser.GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await browser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}