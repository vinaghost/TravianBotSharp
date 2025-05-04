using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class SleepTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            if (taskManager.IsExist<SleepTask.Task>(accountId)) return;
            using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var workTime = context.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            await taskManager.Add<SleepTask.Task>(accountId, executeTime: DateTime.Now.AddMinutes(workTime));
        }
    }
}