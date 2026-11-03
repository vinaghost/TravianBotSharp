using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class UpdateFarmListTask(
        ToFarmListPageCommand.Handler toFarmListPageCommand,
        UpdateFarmlistCommand.Handler updateFarmlistCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => nameof(UpdateFarmListTask).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
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