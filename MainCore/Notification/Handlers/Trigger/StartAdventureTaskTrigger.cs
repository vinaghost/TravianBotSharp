using MainCore.Notification.Base;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class StartAdventureTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();
            var autoStartAdventure = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
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