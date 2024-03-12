using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class InventoryRefresh : INotificationHandler<HeroItemUpdated>
    {
        private readonly HeroViewModel _viewModel;

        public InventoryRefresh(HeroViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(HeroItemUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.InventoryRefresh(notification.AccountId);
        }
    }
}