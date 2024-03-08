using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class AdventureRefresh : INotificationHandler<AdventureUpdated>
    {
        private readonly HeroViewModel _viewModel;

        public AdventureRefresh(HeroViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(AdventureUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.AdventureRefresh(notification.AccountId);
        }
    }
}