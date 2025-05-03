using MainCore.Commands.Features.ClaimQuest;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class ClaimQuestTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName, DateTime executeAt) : base(accountId, villageId, villageName, executeAt)
            {
            }

            protected override string TaskName => "Claim quest";
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
            result = await claimQuestCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}