namespace MainCore.Features.TrainTroop;

[Handler]
public sealed partial class TrainTroopCommand(ILogger logger)
{
    public sealed record Command(IChromeBrowser Browser, TroopEnums Troop, int Amount, bool TrainWhenLowResource) : ICommand
    {
        public string Name => $"{nameof(TrainTroopCommand).Humanize()} [{Description}]";
        private string Description => $"Train {Amount} {Troop}";
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, troop, amount, trainWhenLowResource) = command;

        var maxAmount = await browser.CurrentPage.GetMaxAmount(troop);
        if (maxAmount == 0)
        {
            return MissingResource.Error(troop);
        }

        if (amount > maxAmount && !trainWhenLowResource)
        {
            return MissingResource.Error(troop);
        }
        Result result;
        result = await browser.Input(browser.CurrentPage.GetInputBox(troop), $"{amount}");
        if (result.IsFailed) return result;

        result = await browser.Click(browser.CurrentPage.GetTrainButton());
        if (result.IsFailed) return result;

        logger.Information("Train {Amount} {Troop} is done.", amount, troop);
        return Result.Ok();
    }
}