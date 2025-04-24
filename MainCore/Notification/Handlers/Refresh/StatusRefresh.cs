using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class StatusRefresh
    {
        private static async ValueTask HandleAsync(
            StatusUpdated notification,
            MainLayoutViewModel mainLayoutViewModel,
            CancellationToken cancellationToken)
        {
            await mainLayoutViewModel.LoadStatus(notification.AccountId, notification.Status);
        }
    }
}