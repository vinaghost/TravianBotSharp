namespace MainCore.Commands.Features
{
    [Handler]
    public sealed partial class TrainTroopCommand(
        ILogger logger,
        IChromeBrowser browser)
    {
        public sealed record Command(TroopEnums Troop, int Amount, bool TrainWhenLowResource);

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (troop, amount, trainWhenLowResource) = command;
            Result result;

            var maxAmount = await TrainTroopParser.GetMaxAmount(browser.CurrentPage, troop);
            if (maxAmount == 0)
            {
                return MissingResource.Error(troop);
            }

            if (amount > maxAmount && !trainWhenLowResource)
            {
                return MissingResource.Error(troop);
            }

            result = await Train(troop, amount);
            if (result.IsFailed) return result;

            logger.Information("Troop training for {Troop} with amount {Amount} is done.", troop, amount);
            return Result.Ok();
        }

        private async ValueTask<Result> Train(TroopEnums troop, long amount)
        {
            Result result;
            result = await browser.Input(TrainTroopParser.GetInputBox(browser.CurrentPage, troop), $"{amount}");
            if (result.IsFailed) return result;

            result = await browser.Click(TrainTroopParser.GetTrainButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}