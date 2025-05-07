using MainCore.Constraints;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notifications.Handlers.Refresh
{
    [Handler]
    public static partial class TaskListRefresh
    {
        private static async ValueTask HandleAsync(
            IAccountNotification notification,
            DebugViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.TaskListRefresh(notification.AccountId);
        }
    }
}