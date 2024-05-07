using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    public class VillageListRefresh : INotificationHandler<VillageUpdated>
    {
        private readonly VillageViewModel _viewModel;

        public VillageListRefresh(VillageViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(VillageUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.VillageListRefresh(notification.AccountId);
        }
    }
}