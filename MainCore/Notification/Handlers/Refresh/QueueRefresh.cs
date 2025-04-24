using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class QueueRefresh
    {
        private static async ValueTask HandleAsync(
            QueueBuildingUpdated notification,
            BuildViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.QueueRefresh(notification.VillageId);
        }
    }
}