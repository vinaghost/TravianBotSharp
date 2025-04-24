using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class UpgradeBuildingTaskTrigger
    {
        private static async ValueTask HandleAsync(
            AccountInit notification,
            ITaskManager taskManager,
            GetVillage getVillage,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var hasBuildingJobVillages = getVillage.HasBuildingJob(accountId);
            foreach (var village in hasBuildingJobVillages)
            {
                await Trigger(accountId, village, taskManager);
            }
        }

        private static async ValueTask HandleAsync(
            JobUpdated notification,
            ITaskManager taskManager,
            GetVillage getVillage,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId, taskManager);
        }

        private static async ValueTask HandleAsync(
            CompleteImmediatelyMessage notification,
            ITaskManager taskManager,
            GetVillage getVillage,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId, taskManager);
        }

        private static async Task Trigger(
            AccountId accountId,
            VillageId villageId,
            ITaskManager taskManager)
        {
            await taskManager.AddOrUpdate<UpgradeBuildingTask>(accountId, villageId);
        }
    }
}