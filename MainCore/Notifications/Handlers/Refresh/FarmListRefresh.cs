using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class FarmListRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            FarmingViewModel farmingViewModel,
            CancellationToken cancellationToken)
        {
            await farmingViewModel.FarmListRefresh(notification.AccountId);
        }
    }
}