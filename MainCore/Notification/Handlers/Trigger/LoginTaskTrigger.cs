using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class LoginTaskTrigger
    {
        private static async ValueTask HandleAsync(
            AccountInit notification,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, taskManager);
        }

        private static async ValueTask HandleAsync(
            AccountLogout notification,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, taskManager);
        }

        private static async Task Trigger(AccountId accountId, ITaskManager taskManager)
        {
            await taskManager.AddOrUpdate<LoginTask>(accountId, first: true);
        }
    }
}