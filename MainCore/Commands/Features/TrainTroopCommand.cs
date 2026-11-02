namespace MainCore.Commands.Features
{
    [Handler]
    public sealed partial class TrainTroopCommand(
        ILogger logger,
        ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId, TroopEnums Troop, int Amount, bool TrainWhenLowResource) : IAccountCommand
        {
            public string Name => $"{nameof(TrainTroopCommand)} [{Description}]";
            private string Description => $"Train {Amount} {Troop}";
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (accountId, troop, amount, trainWhenLowResource) = command;
            var browser = chromeManager.Get(accountId);

            var maxAmount = await TrainTroopParser.GetMaxAmount(browser.CurrentPage, troop);
            if (maxAmount == 0)
            {
                return MissingResource.Error(troop);
            }

            if (amount > maxAmount && !trainWhenLowResource)
            {
                return MissingResource.Error(troop);
            }
            Result result;
            result = await browser.Input(TrainTroopParser.GetInputBox(browser.CurrentPage, troop), $"{amount}");
            if (result.IsFailed) return result;

            result = await browser.Click(TrainTroopParser.GetTrainButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            logger.Information("Train {Amount} {Troop} is done.", amount, troop);
            return Result.Ok();
        }
    }
}