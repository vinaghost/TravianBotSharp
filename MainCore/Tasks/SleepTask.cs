using MainCore.Commands.Features;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class SleepTask
    {
        public sealed class Task : AccountTask
        {
            public Task(AccountId accountId) : base(accountId)
            {
            }

            protected override string TaskName => "Sleep";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            IDbContextFactory<AppDbContext> contextFactory,
            ITaskManager taskManager,
            SleepCommand.Handler sleepCommand,
            GetAccessQuery.Handler getAccessQuery,
            OpenBrowserCommand.Handler openBrowserCommand,
            ToDorfCommand.Handler toDorfCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await sleepCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            var (_, isFailed, access, errors) = await getAccessQuery.HandleAsync(new(task.AccountId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            await openBrowserCommand.HandleAsync(new(task.AccountId, access), cancellationToken);

            using var context = await contextFactory.CreateDbContextAsync();
            var workTime = context.ByName(task.AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            task.ExecuteAt = DateTime.Now.AddMinutes(workTime);
            await taskManager.ReOrder(task.AccountId);

            return Result.Ok();
        }
    }
}