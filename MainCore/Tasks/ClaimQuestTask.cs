using MainCore.Commands.Features.ClaimQuest;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class ClaimQuestTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "Claim quest";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoClaimQuestEnable);
                if (!settingEnable) return false;

                return true;
            }
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToQuestPageCommand.Handler toQuestPageCommand,
            ClaimQuestCommand.Handler claimQuestCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toQuestPageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            result = await claimQuestCommand.HandleAsync(new(), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}