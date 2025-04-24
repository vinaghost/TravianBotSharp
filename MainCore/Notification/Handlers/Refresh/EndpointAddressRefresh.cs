using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class EndpointAddressRefresh
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            DebugViewModel debugViewModel,
            CancellationToken cancellationToken)
        {
            await debugViewModel.EndpointAddressRefresh(notification.AccountId);
        }
    }
}