using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class LoginTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            taskManager.AddOrUpdate<LoginTask.Task>(new(accountId), first: true);
        }
    }
}