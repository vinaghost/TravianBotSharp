using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class HeroRefresh : INotificationHandler<HeroUpdated>
    {
        private readonly HeroViewModel _viewModel;

        public HeroRefresh(HeroViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(HeroUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.HeroRefresh(notification.AccountId);
        }
    }
}