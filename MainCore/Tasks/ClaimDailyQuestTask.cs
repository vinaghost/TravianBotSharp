using MainCore.Commands.Features.ClaimDailyQuest;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class ClaimDailyQuestTask(
        ChromeManager chromeManager,
        CheckDailyQuestCommand.Handler checkDailyQuestCommand,
        OpenDailQuestPanelCommand.Handler openDailQuestPanelCommand,
        ClaimRewardCommand.Handler claimRewardCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => nameof(ClaimDailyQuestTask).Humanize();

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoClaimDailyQuest);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(task.AccountId);
            Result result;
            var canClaimQuest = await checkDailyQuestCommand.HandleAsync(new(browser), cancellationToken);
            if (!canClaimQuest) return Result.Ok();

            result = await openDailQuestPanelCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            result = await claimRewardCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}