using MainCore.Commands.Features.CompleteImmediately;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class CompleteImmediatelyTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName) : base(accountId, villageId, villageName)
            {
            }

            protected override string TaskName => "Complete immediately";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToDorfCommand.Handler toDorfCommand,
            CompleteImmediatelyCommand.Handler completeImmediatelyCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toDorfCommand.HandleAsync(new(task.AccountId, 0), cancellationToken);
            if (result.IsFailed) return result;
            result = await completeImmediatelyCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}