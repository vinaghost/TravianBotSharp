using MainCore.UI.ViewModels.UserControls;
using System.Reactive.Linq;

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
            await _mainlayoutViewModel.LoadAccount.Execute();
        }
    }
}