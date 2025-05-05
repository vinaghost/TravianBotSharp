using MainCore.Notification.Base;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class FarmListRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            FarmingViewModel farmingViewModel,
            CancellationToken cancellationToken)
        {
            await farmingViewModel.FarmListRefresh(notification.AccountId);
        }
    }
}