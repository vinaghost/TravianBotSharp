using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class StartAdventureTaskTrigger
    {
        private static async ValueTask HandleAsync(
            AdventureUpdated notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, taskManager, getSetting);
        }

        private static async ValueTask HandleAsync(
            AccountInit notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, taskManager, getSetting);
        }

        private static async ValueTask HandleAsync(
            AccountSettingUpdated notification,
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
            var autoStartAdventure = getSetting.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
            if (autoStartAdventure)
            {
                if (taskManager.IsExist<StartAdventureTask>(accountId)) return;
                await taskManager.Add<StartAdventureTask>(accountId);
            }
            else
            {
                var task = taskManager.Get<StartAdventureTask>(accountId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}