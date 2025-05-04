using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class LoginTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            await taskManager.AddOrUpdate<LoginTask.Task>(accountId, first: true);
        }
    }
}