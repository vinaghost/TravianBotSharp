using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class JobListRefresh
    {
        private static async ValueTask HandleAsync(
            IVillageNotification notification,
            BuildViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.JobListRefresh(notification.VillageId);
        }
    }
}