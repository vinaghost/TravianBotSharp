using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class SleepTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            ITaskManager taskManager,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            if (taskManager.IsExist<SleepTask.Task>(accountId)) return;

            var workTime = settingService.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            var sleepTask = new SleepTask.Task(accountId);
            sleepTask.ExecuteAt = DateTime.Now.AddMinutes(workTime);
            taskManager.Add<SleepTask.Task>(sleepTask);
        }
    }
}