using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class AccountSettingRefresh : INotificationHandler<AccountSettingUpdated>
    {
        private readonly AccountSettingViewModel _viewModel;

        public AccountSettingRefresh(AccountSettingViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(AccountSettingUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.SettingRefresh(notification.AccountId);
        }
    }
}