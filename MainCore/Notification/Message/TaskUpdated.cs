using MainCore.Constraints;
using MainCore.Notification.Handlers.Refresh;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class TaskUpdated
    {
        public sealed record Notification(AccountId AccountId) : IAccountNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            TaskListRefresh.Handler taskListRefresh,
            CancellationToken cancellationToken)
        {
            await taskListRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}