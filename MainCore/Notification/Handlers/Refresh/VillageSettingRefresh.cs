using MainCore.Notification.Base;
using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class VillageSettingRefresh
    {
        private static async ValueTask HandleAsync(
            IVillageNotification notification,
            VillageSettingViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.SettingRefresh(notification.VillageId);
        }
    }
}