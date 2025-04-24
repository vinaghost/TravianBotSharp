using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class SleepTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            if (taskManager.IsExist<SleepTask>(accountId)) return;
            var workTime = getSetting.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            await taskManager.Add<SleepTask>(accountId, executeTime: DateTime.Now.AddMinutes(workTime));
        }
    }
}