using MainCore.Constraints;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class StartAdventureTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            ITaskManager taskManager,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;

            var autoStartAdventure = settingService.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
            if (autoStartAdventure)
            {
                if (taskManager.IsExist<StartAdventureTask.Task>(accountId)) return;
                await taskManager.Add<StartAdventureTask.Task>(new(accountId));
            }
            else
            {
                var task = taskManager.Get<StartAdventureTask.Task>(accountId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}