using Immediate.Handlers.Shared;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class AccountSettingRefreshTrigger
    {
        private static async ValueTask HandleAsync(
            AccountSettingUpdated @event,
            AccountSettingViewModel viewModel,
            CancellationToken cancellationToken
        )
        {
            await viewModel.SettingRefresh(@event.AccountId);
        }
    }
}