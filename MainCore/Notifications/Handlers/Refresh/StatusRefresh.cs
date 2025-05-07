using MainCore.Constraints;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class StatusRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            MainLayoutViewModel mainLayoutViewModel,
            CancellationToken cancellationToken)
        {
            await mainLayoutViewModel.LoadStatus(notification.AccountId);
        }
    }
}