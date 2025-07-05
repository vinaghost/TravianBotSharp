using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class UpgradeBuildingTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
            taskManager.AddOrUpdate<UpgradeBuildingTask.Task>(new(accountId, villageId, villageName));
        }
    }
}