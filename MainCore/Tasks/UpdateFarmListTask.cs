using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class UpdateFarmListTask(
        ToFarmListPageCommand.Handler toFarmListPageCommand,
        UpdateFarmlistCommand.Handler updateFarmlistCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Update farm list";
        }

        private async ValueTask<Result> HandleAsync(
            Task task,
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