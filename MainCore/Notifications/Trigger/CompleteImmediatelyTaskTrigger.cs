using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class CompleteImmediatelyTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            AppDbContext context,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            if (taskManager.IsExist<CompleteImmediatelyTask.Task>(accountId, villageId)) return;

            var count = context.Count(villageId);
            if (count == 0) return;

            var completeImmediatelyEnable = context.BooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
            if (!completeImmediatelyEnable) return;

            if (!context.IsSkippableBuilding(villageId)) return;

            var completeImmediatelyTime = context.ByName(villageId, VillageSettingEnums.CompleteImmediatelyTime);

            var requiredTime = DateTime.Now.AddMinutes(completeImmediatelyTime);
            var queueTime = context.GetQueueTime(villageId);

            if (requiredTime > queueTime) return;
            var villageName = context.GetVillageName(villageId);
            taskManager.Add<CompleteImmediatelyTask.Task>(new(accountId, villageId, villageName));
        }

        private static DateTime GetQueueTime(this AppDbContext context, VillageId villageId)
        {
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .OrderByDescending(x => x.CompleteTime)
                .Select(x => x.CompleteTime)
                .FirstOrDefault();
            return queueBuilding;
        }

        private static bool IsSkippableBuilding(this AppDbContext context, VillageId villageId)
        {
            var buildings = new List<BuildingEnums>
            {
                BuildingEnums.Site,
                BuildingEnums.Residence,
                BuildingEnums.Palace,
                BuildingEnums.CommandCenter,
            };

            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => !buildings.Contains(x.Type))
                .Any();
            return queueBuilding;
        }

        private static int Count(this AppDbContext context, VillageId villageId)
        {
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Count();
            return count;
        }
    }
}