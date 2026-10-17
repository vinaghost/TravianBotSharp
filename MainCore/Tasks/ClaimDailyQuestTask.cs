using MainCore.Commands.Features.ClaimDailyQuest;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class ClaimDailyQuestTask(
        IChromeBrowser browser,
        OpenDailQuestPanelCommand.Handler openDailQuestPanelCommand,
        ClaimRewardCommand.Handler claimRewardCommand)
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
            Task task,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
            CancellationToken cancellationToken
           )
        {
            Result result;
            var canClaimQuest = await QuestParser.IsDailyQuestClaimable(browser.CurrentPage);
            if (!canClaimQuest) return Result.Ok();

            result = await openDailQuestPanelCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            result = await claimRewardCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}