namespace MainCore.Commands.Features.ClaimDailyQuest
{
    [Handler]
    public sealed partial class ClaimRewardCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(ClaimRewardCommand);
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);

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