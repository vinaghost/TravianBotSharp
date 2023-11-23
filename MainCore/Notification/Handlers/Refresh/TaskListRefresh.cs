using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class TaskListRefresh : INotificationHandler<TaskUpdated>
    {
        private readonly DebugViewModel _viewModel;

        public TaskListRefresh(DebugViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task Handle(TaskUpdated notification, CancellationToken cancellationToken)
        {
            await _viewModel.TaskListRefresh(notification.AccountId);
        }
    }
}