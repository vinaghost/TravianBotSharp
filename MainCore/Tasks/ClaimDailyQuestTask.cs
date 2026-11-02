using MainCore.Commands.Features.ClaimDailyQuest;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class ClaimDailyQuestTask(
        CheckDailyQuestCommand.Handler checkDailyQuestCommand,
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

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            Result result;
            var canClaimQuest = await checkDailyQuestCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (!canClaimQuest) return Result.Ok();

            result = await openDailQuestPanelCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await claimRewardCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}