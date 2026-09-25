using MainCore.Commands.Features;
using MainCore.Commands.NextExecute;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class SleepTask(
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeBrowser browser,
        ILogger logger,
        GetValidAccessCommand.Handler getAccessQuery,
        OpenBrowserCommand.Handler openBrowserCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Sleep";
        }

        private async ValueTask<Result> HandleAsync(
            Task task,
            CancellationToken cancellationToken)
        {
            await browser.Shutdown();
            await Sleep(task.AccountId, cancellationToken);

            var (_, isFailed, access, errors) = await getAccessQuery.HandleAsync(new(task.AccountId), cancellationToken);
            if (isFailed) return Result.Fail(errors);
            await openBrowserCommand.HandleAsync(new(task.AccountId, access), cancellationToken);

            task.ExecuteAt = GetNextExecuteTime(task.AccountId);
            return Result.Ok();
        }

        private async Task<Result> Sleep(AccountId accountId, CancellationToken cancellationToken)
        {
            var sleepEnd = GetSleepTime(accountId);
            int lastMinute = 0;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var timeRemaining = sleepEnd - DateTime.Now;
                if (timeRemaining < TimeSpan.Zero) return Result.Ok();
                try
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return Cancel.Error;
                }

                var currentMinute = (int)timeRemaining.TotalMinutes;
                if (lastMinute != currentMinute)
                {
                    logger.Information("Chrome will reopen in {CurrentMinute} mins", currentMinute);
                    lastMinute = currentMinute;
                }
            }
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