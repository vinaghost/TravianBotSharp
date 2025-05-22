using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class VillageSettingRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            VillageSettingViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.SettingRefresh(notification.VillageId);
        }
    }
}