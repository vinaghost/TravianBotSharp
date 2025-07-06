using MainCore.Constraints;
using MainCore.Specifications;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class CompleteImmediatelyTaskTrigger
    {
        private static List<BuildingEnums> UnskippableBuildings = new()
        {
            BuildingEnums.Residence,
            BuildingEnums.Palace,
            BuildingEnums.CommandCenter,
        };

        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            AppDbContext context,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (accountId, villageId) = notification;

            var taskExist = taskManager.IsExist<CompleteImmediatelyTask.Task>(accountId, villageId);
            if (taskExist) return;

            var settingEnable = context.BooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
            if (!settingEnable) return;

            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            // empty
            if (queueBuildings.Count == 0) return;
            // contains unskippable building
            if (queueBuildings.Any(x => UnskippableBuildings.Contains(x.Type))) return;

            var completeImmediatelyMinimumTime = context.ByName(villageId, VillageSettingEnums.CompleteImmediatelyTime);
            var requiredTime = DateTime.Now.AddMinutes(completeImmediatelyMinimumTime);

            var firstQueueBuildingCompleteTime = queueBuildings
                .OrderBy(x => x.CompleteTime)
                .Select(x => x.CompleteTime)
                .FirstOrDefault();

            if (requiredTime > firstQueueBuildingCompleteTime) return;

            var getVillageSpec = new GetVillageNameSpec(villageId);
            var villageName = context.Villages
                .WithSpecification(getVillageSpec)
                .First();
            taskManager.Add<CompleteImmediatelyTask.Task>(new(accountId, villageId, villageName));
        }
    }
}