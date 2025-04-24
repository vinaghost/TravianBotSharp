using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class BuildingListRefresh
    {
        private static async ValueTask HandleAsync(
            BuildingUpdated notification,
            BuildViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.BuildingListRefresh(notification.VillageId);
        }
    }
}