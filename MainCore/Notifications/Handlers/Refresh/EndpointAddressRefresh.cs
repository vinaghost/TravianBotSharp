using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class EndpointAddressRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            DebugViewModel debugViewModel,
            CancellationToken cancellationToken)
        {
            await debugViewModel.EndpointAddressRefresh(notification.AccountId);
        }
    }
}