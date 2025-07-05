using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class UpgradeBuildingTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
           AppDbContext context,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (accountId, villageId) = notification;
            var villageName = context.GetVillageName(villageId);
            taskManager.AddOrUpdate<UpgradeBuildingTask.Task>(new(accountId, villageId, villageName));
        }
    }
}