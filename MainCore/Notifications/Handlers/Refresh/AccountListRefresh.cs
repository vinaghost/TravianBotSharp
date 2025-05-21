using MainCore.Constraints;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class AccountListRefresh
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            MainLayoutViewModel mainLayoutViewModel,
            CancellationToken cancellationToken)
        {
            await mainLayoutViewModel.LoadAccountCommand.Execute();
        }
    }
}