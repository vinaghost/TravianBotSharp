using MainCore.Commands.Features;
using MainCore.Commands.NextExecute;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class SleepTask
    {
        public sealed record Task : AccountTask
        {
            public Task(AccountId accountId) : base(accountId)
            {
            }

            protected override string TaskName => "Sleep";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            SleepCommand.Handler sleepCommand,
            GetValidAccessCommand.Handler getAccessQuery,
            OpenBrowserCommand.Handler openBrowserCommand,
            ToDorfCommand.Handler toDorfCommand,
            NextExecuteSleepTaskCommand.Handler nextExecuteSleepTaskCommand,
            CancellationToken cancellationToken)
        {
            await sleepCommand.HandleAsync(new(task.AccountId), cancellationToken);
            var (_, isFailed, access, errors) = await getAccessQuery.HandleAsync(new(task.AccountId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            await openBrowserCommand.HandleAsync(new(task.AccountId, access), cancellationToken);
            await nextExecuteSleepTaskCommand.HandleAsync(task, cancellationToken);
            return Result.Ok();
        }
    }
}