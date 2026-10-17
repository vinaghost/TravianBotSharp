namespace MainCore.Commands.Features.ClaimDailyQuest
{
    [Handler]
    public static partial class OpenDailQuestPanelCommand
    {
        public sealed record Command(IChromeBrowser Browser);

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
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