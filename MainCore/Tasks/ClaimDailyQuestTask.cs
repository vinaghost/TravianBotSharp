using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class ClaimDailyQuestTask(IChromeBrowser browser)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Claim daily quest";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoClaimDailyQuest);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(
#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable IDE0060 // Remove unused parameter
            Task task
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
           )
        {
            Result result;
            var canClaimQuest = await QuestParser.IsDailyQuestClaimable(browser.CurrentPage);
            if (!canClaimQuest) return Result.Ok();
            result = await ToQuestPage();
            if (result.IsFailed) return result;
            result = await ClaimQuest();
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask<Result> ToQuestPage()
        {
            var questDaily = NavigationBarParser.GetDailyQuestButton(browser.CurrentPage);

            var result = await browser.Click(questDaily);
            if (result.IsFailed) return result;

            var randomQuest = browser.CurrentPage.Locator("#dailyQuests .screen.active .dailyQuest:not(.placeholder)");
            result = await browser.Wait(randomQuest.First);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask<Result> ClaimQuest()
        {
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