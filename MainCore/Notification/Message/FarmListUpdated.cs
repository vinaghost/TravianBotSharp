using MainCore.Notification.Handlers.Refresh;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class FarmListUpdated
    {
        public sealed record Notification(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            FarmListRefresh.Handler farmListRefresh,
            CancellationToken cancellationToken)
        {
            await farmListRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}