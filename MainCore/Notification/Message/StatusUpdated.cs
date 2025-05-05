using MainCore.Notification.Base;
using MainCore.Notification.Handlers.Refresh;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class StatusUpdated
    {
        public sealed record Notification(AccountId AccountId) : IAccountNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            StatusRefresh.Handler statusRefresh,
            EndpointAddressRefresh.Handler endpointAddressRefresh,
            CancellationToken cancellationToken)
        {
            await statusRefresh.HandleAsync(notification, cancellationToken);
            await endpointAddressRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}