using MainCore.Constraints;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class LoginTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            await taskManager.AddOrUpdate<LoginTask.Task>(new(accountId), first: true);
        }
    }
}