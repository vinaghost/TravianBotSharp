using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class StartAdventureTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();
            var autoStartAdventure = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
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