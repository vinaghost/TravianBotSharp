using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class BuildingListRefresh
    {
        private static async ValueTask HandleAsync(
            IVillageConstraint notification,
            BuildViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.BuildingListRefresh(notification.VillageId);
        }
    }
}