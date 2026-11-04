using MainCore.Commands.Features.Sleep;
using MainCore.Commands.Misc;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class SleepTask(
        IDbContextFactory<AppDbContext> contextFactory,
        OpenBrowserCommand.Handler openBrowserCommand,
        CloseBrowserCommand.Handler closeBrowserCommand,
        SleepCommand.Handler sleepCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => nameof(SleepTask).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            await closeBrowserCommand.HandleAsync(new(task.AccountId), cancellationToken);

            var sleepEnd = GetSleepTime(task.AccountId);
            var result = await sleepCommand.HandleAsync(new(sleepEnd), cancellationToken);
            if (result.IsFailed) return result;

            result = await openBrowserCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            task.ExecuteAt = GetNextExecuteTime(task.AccountId);
            return Result.Ok();
        }

        private DateTime GetSleepTime(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var sleepTime = context.ByName(
                accountId,
                AccountSettingEnums.SleepTimeMin,
                AccountSettingEnums.SleepTimeMax,
                60);
            var sleepEnd = DateTime.Now.AddSeconds(sleepTime);

            return sleepEnd;
        }

        private DateTime GetNextExecuteTime(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var workTime = context.ByName(
                accountId,
                AccountSettingEnums.WorkTimeMin,
                AccountSettingEnums.WorkTimeMax,
                60);
            var sleepNext = DateTime.Now.AddSeconds(workTime);
            return sleepNext;
        }
    }
}