using MainCore.Notification.Base;
using MainCore.Notification.Handlers.Refresh;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class AccountUpdated
    {
        public sealed record Notification : INotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            AccountListRefresh.Handler accountListRefresh,
            CancellationToken cancellationToken)
        {
            await accountListRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}