using MainCore.UI.ViewModels.Tabs;

namespace MainCore.Notification.Handlers.Refresh
{
    [Handler]
    public static partial class TaskListRefresh
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            DebugViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await viewModel.TaskListRefresh(notification.AccountId);
        }
    }
}