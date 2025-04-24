using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class TaskListRefresh
    {
        private static async ValueTask HandleAsync(
            TaskUpdated notification,
            DebugViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.TaskListRefresh(notification.AccountId);
        }
    }
}