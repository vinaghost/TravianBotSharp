using MainCore.Notification.Message;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class AccountListRefresh : INotificationHandler<AccountUpdated>
    {
        private readonly MainLayoutViewModel _mainlayoutViewModel;

        public AccountListRefresh(MainLayoutViewModel mainlayoutViewModel)
        {
            _mainlayoutViewModel = mainlayoutViewModel;
        }

        public async Task Handle(AccountUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            _mainlayoutViewModel.LoadAccountList();
        }
    }
}