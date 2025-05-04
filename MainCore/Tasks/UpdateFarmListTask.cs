using MainCore.Commands.Features.StartFarmList;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class UpdateFarmListTask
    {
        public sealed class Task : AccountTask
        {
            public Task(AccountId accountId) : base(accountId)
            {
            }

            protected override string TaskName => "Update farm list";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToFarmListPageCommand.Handler toFarmListPageCommand,
            UpdateFarmlistCommand.Handler updateFarmlistCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toFarmListPageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            result = await updateFarmlistCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}