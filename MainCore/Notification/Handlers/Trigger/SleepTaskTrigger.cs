using MainCore.Constraints;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class SleepTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            ITaskManager taskManager,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            if (taskManager.IsExist<SleepTask.Task>(accountId)) return;
            
            var workTime = context.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            var sleepTask = new SleepTask.Task(accountId);
            sleepTask.ExecuteAt = DateTime.Now.AddMinutes(workTime);
            await taskManager.Add<SleepTask.Task>(sleepTask);
        }
    }
}