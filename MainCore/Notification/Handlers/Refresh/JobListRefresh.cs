using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notification.Handlers.Refresh
{
    internal class JobListRefresh : INotificationHandler<JobUpdated>
    {
        private readonly BuildViewModel _viewModel;

        public JobListRefresh(BuildViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(JobUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.JobListRefresh(notification.VillageId);
        }
    }
}