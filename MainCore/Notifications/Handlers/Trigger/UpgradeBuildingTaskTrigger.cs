using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class UpgradeBuildingTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageNotification notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
            await taskManager.AddOrUpdate<UpgradeBuildingTask.Task>(new(accountId, villageId, villageName));
        }
    }
}