namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public sealed partial class StartActiveFarmlistCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(StartAllFarmlistCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var result = await browser.Click(FarmListParser.GetStartAllButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}