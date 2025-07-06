using MainCore.Constraints;
using MainCore.Specifications;

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
            var getVillageSpec = new GetVillageNameSpec(villageId);
            var villageName = context.Villages
                .WithSpecification(getVillageSpec)
                .First();

            taskManager.AddOrUpdate<UpgradeBuildingTask.Task>(new(accountId, villageId, villageName));
        }
    }
}