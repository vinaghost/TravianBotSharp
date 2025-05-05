using MainCore.Commands.Base;
using MainCore.Errors.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class TrainTroopCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, BuildingEnums Building) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            IDbContextFactory<AppDbContext> contextFactory,
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

            result = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return result;

            var buildingLocation = await getBuildingLocation.HandleAsync(new(villageId, building), cancellationToken);
            if (buildingLocation == default)
            {
                return MissingBuilding.Error(building);
            }

            result = await toBuildingCommand.HandleAsync(new(accountId, buildingLocation), cancellationToken);
            if (result.IsFailed) return result;

            using var context = await contextFactory.CreateDbContextAsync();
            var troopSetting = BuildingSettings[building];
            var troop = (TroopEnums)context.ByName(villageId, troopSetting);

            var browser = chromeManager.Get(accountId);

            var (_, isFailed, amount, errors) = GetAmount(context, browser, villageId, building, troop);
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
            AppDbContext context,
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
            var amount = context.ByName(villageId, minSetting, maxSetting);
            if (amount < maxAmount)
            {
                return amount;
            }

            var trainWhenLowResource = context.BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
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