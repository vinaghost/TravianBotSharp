using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class StartFarmListTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;

            var taskExist = taskManager.IsExist<StartFarmListTask.Task>(accountId);
            if (taskExist) return;

            taskManager.Add<StartFarmListTask.Task>(new(accountId));
        }
    }
}