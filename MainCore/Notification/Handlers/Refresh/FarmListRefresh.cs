using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class FarmListRefresh
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            FarmingViewModel farmingViewModel,
            CancellationToken cancellationToken)
        {
            await farmingViewModel.FarmListRefresh(notification.AccountId);
        }
    }
}