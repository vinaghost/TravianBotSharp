using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class FarmListRefresh : INotificationHandler<FarmListUpdated>
    {
        private readonly FarmingViewModel _viewModel;

        public FarmListRefresh(FarmingViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(FarmListUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.FarmListRefresh(notification.AccountId);
        }
    }
}