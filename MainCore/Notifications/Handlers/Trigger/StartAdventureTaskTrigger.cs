using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class StartAdventureTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            ITaskManager taskManager,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;

            var autoStartAdventure = settingService.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
            if (autoStartAdventure)
            {
                if (taskManager.IsExist<StartAdventureTask.Task>(accountId)) return;
                taskManager.Add<StartAdventureTask.Task>(new(accountId));
            }
            else
            {
                taskManager.Remove<StartAdventureTask.Task>(accountId);
            }
        }
    }
}