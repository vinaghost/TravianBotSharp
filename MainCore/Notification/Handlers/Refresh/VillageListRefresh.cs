using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class VillageListRefresh
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            VillageViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.VillageListRefresh(notification.AccountId);
        }
    }
}