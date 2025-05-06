using MainCore.Constraints;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class CompleteImmediatelyTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IVillageNotification notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            if (taskManager.IsExist<CompleteImmediatelyTask.Task>(accountId, villageId)) return;
            

            context.Clean(villageId);

            var count = context.Count(villageId);
            if (count == 0) return;

            var completeImmediatelyEnable = context.BooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
            if (!completeImmediatelyEnable) return;

            if (!context.IsSkippableBuilding(villageId)) return;

            var completeImmediatelyTime = context.ByName(villageId, VillageSettingEnums.CompleteImmediatelyTime);

            var requiredTime = DateTime.Now.AddMinutes(completeImmediatelyTime);
            var queueTime = context.GetQueueTime(villageId);

            if (requiredTime > queueTime) return;
            var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
            await taskManager.Add<CompleteImmediatelyTask.Task>(new(accountId, villageId, villageName));
        }

        private static DateTime GetQueueTime(this AppDbContext context, VillageId villageId)
        {
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .OrderByDescending(x => x.Position)
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

        private static void Clean(this AppDbContext context, VillageId villageId)
        {
            var now = DateTime.Now;
            var completeBuildingQuery = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Where(x => x.CompleteTime < now);

            var completeBuildingLocations = completeBuildingQuery
                .Select(x => x.Location)
                .ToList();

            foreach (var completeBuildingLocation in completeBuildingLocations)
            {
                context.Buildings
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Location == completeBuildingLocation)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Level, x => x.Level + 1));
            }

            completeBuildingQuery
                .ExecuteUpdate(x => x.SetProperty(x => x.Type, BuildingEnums.Site));
        }

        private static int Count(this AppDbContext context, VillageId villageId)
        {
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }
    }
}