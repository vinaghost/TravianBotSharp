using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class AccountListRefresh
    {
        private static async ValueTask HandleAsync(
            AccountUpdated notification,
            MainLayoutViewModel mainLayoutViewModel,
            CancellationToken cancellationToken)
        {
            await mainLayoutViewModel.LoadAccountCommand.Execute();
        }
    }
}