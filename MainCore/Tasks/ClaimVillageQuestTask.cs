using MainCore.Commands.Features.ClaimVillageQuest;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class ClaimVillageQuestTask(
        ChromeManager chromeManager,
        CheckVillageQuestCommand.Handler checkVillageQuestCommand,
        OpenVillageQuestPageCommand.Handler openVillageQuestPageCommand,
        ClaimRewardCommand.Handler claimRewardCommand)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => nameof(ClaimVillageQuestTask).Humanize();

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoClaimQuestEnable);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(task.AccountId);
            Result result;
            var canClaimQuest = await checkVillageQuestCommand.HandleAsync(new(browser), cancellationToken);
            if (!canClaimQuest) return Result.Ok();

            result = await openVillageQuestPageCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            result = await claimRewardCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}