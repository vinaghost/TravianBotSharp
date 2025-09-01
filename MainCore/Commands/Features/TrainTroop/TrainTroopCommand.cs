#pragma warning disable S1172

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class TrainTroopCommand
    {
        public sealed record Command(VillageId VillageId, BuildingEnums Building) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (villageId, building) = command;
            var troop = (TroopEnums)context.ByName(villageId, TroopSettings[building]);
            var maxAmount = TrainTroopParser.GetMaxAmount(browser.Html, troop);
            if (maxAmount == 0)
            {
                return MissingResource.Error(troop);
            }

            var (minSetting, maxSetting) = AmountSettings[building];
            var amount = context.ByName(villageId, minSetting, maxSetting);
            if (amount > maxAmount)
            {
                var trainWhenLowResource = context.BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
                if (!trainWhenLowResource)
                {
                    return MissingResource.Error(troop);
                }
            }

            var result = await TrainTroop(browser, troop, amount, cancellationToken);
            if (result.IsFailed) return result;

            logger.Information("Troop training for {Troop} with amount {Amount} is done.", troop, amount);
            return Result.Ok();
        }

        public static Dictionary<BuildingEnums, VillageSettingEnums> TroopSettings { get; } = new()
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

        private static async ValueTask<Result> TrainTroop(
            IChromeBrowser browser,
            TroopEnums troop,
            long amount,
            CancellationToken cancellationToken)
        {
            var inputBox = TrainTroopParser.GetInputBox(browser.Html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            Result result;
            result = await browser.Input(By.XPath(inputBox.XPath), $"{amount}", cancellationToken);
            if (result.IsFailed) return result;

            var trainButton = TrainTroopParser.GetTrainButton(browser.Html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await browser.Click(By.XPath(trainButton.XPath), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}