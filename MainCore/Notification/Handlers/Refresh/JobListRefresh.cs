using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class JobListRefresh
    {
        private static async ValueTask HandleAsync(
            JobUpdated notification,
            BuildViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.JobListRefresh(notification.VillageId);
        }
    }
}