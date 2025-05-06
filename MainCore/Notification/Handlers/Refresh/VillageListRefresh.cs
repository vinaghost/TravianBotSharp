using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class VillageListRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            VillageViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.VillageListRefresh(notification.AccountId);
        }
    }
}