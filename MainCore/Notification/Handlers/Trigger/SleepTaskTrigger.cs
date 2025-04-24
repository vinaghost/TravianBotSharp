using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class SleepTaskTrigger
    {
        private static async ValueTask HandleAsync(
            AccountInit notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, taskManager, getSetting);
        }

        private static async Task Trigger(
            AccountId accountId,
            ITaskManager taskManager,
            IGetSetting getSetting)
        {
            if (taskManager.IsExist<SleepTask>(accountId)) return;
            var workTime = getSetting.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            await taskManager.Add<SleepTask>(accountId, executeTime: DateTime.Now.AddMinutes(workTime));
        }
    }
}