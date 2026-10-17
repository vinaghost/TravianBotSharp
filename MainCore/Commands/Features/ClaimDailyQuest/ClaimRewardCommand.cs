namespace MainCore.Commands.Features.ClaimDailyQuest
{
    [Handler]
    public static partial class ClaimRewardCommand
    {
        public sealed record Command(IChromeBrowser Browser);

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;

            Result result;

            var collectButtons = QuestParser.GetDailyQuestCollectButton(browser.CurrentPage);
            var collectButton = collectButtons.Locator("svg.above #innerCircle path").First;
            result = await browser.Click(collectButton);
            if (result.IsFailed) return result;

            var claimButton = browser.CurrentPage.Locator("#dailyQuests .screen.active #dailyQuestsRewardScreen button.collect.collectable");
            result = await browser.Click(claimButton);
            if (result.IsFailed) return result;

            result = await browser.Refresh();
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}