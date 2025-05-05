using MainCore.Notification.Base;
using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notification.Handlers.Refresh
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