using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs.Villages;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class QueueRefresh : INotificationHandler<QueueBuildingUpdated>
    {
        private readonly BuildViewModel _viewModel;

        public QueueRefresh(BuildViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(QueueBuildingUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.QueueRefresh(notification.VillageId);
        }
    }
}