namespace MainCore.Commands.Features.ClaimDailyQuest
{
    [Handler]
    public sealed partial class OpenDailQuestPanelCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(OpenDailQuestPanelCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            Result result;

            var dailyQuestButton = NavigationBarParser.GetDailyQuestButton(browser.CurrentPage);
            result = await browser.Click(dailyQuestButton);
            if (result.IsFailed) return result;

            var dailyQuestPanelReadyLocator = browser.CurrentPage.Locator("#dailyQuests .screen.active .dailyQuest:not(.placeholder)").First;
            result = await browser.Wait(dailyQuestPanelReadyLocator);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}