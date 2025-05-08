using MainCore.Constraints;
using MainCore.Notifications.Handlers.Refresh;

namespace MainCore.Notifications.Message
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