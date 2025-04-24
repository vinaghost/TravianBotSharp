using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class AccountSettingRefresh
    {
        private static async ValueTask HandleAsync(
            AccountSettingUpdated notification,
            AccountSettingViewModel viewModel,
            CancellationToken cancellationToken
        )
        {
            await viewModel.SettingRefresh(notification.AccountId);
        }
    }
}