using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class CompleteImmediatelyTaskTrigger
    {
        private static async ValueTask HandleAsync(
            QueueBuildingUpdated notification,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId, taskManager, contextFactory, getSetting);
        }

        private static async ValueTask HandleAsync(
            VillageSettingUpdated notification,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId, taskManager, contextFactory, getSetting);
        }

        private static async Task Trigger(
            AccountId accountId,
            VillageId villageId,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            IGetSetting getSetting)
        {
            if (taskManager.IsExist<CompleteImmediatelyTask>(accountId, villageId)) return;
            Clean(villageId, contextFactory);

            var count = Count(villageId, contextFactory);
            if (count == 0) return;

            var completeImmediatelyEnable = getSetting.BooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
            if (!completeImmediatelyEnable) return;

            if (!IsSkippableBuilding(villageId, contextFactory)) return;

            var completeImmediatelyTime = getSetting.ByName(villageId, VillageSettingEnums.CompleteImmediatelyTime);

            var requiredTime = DateTime.Now.AddMinutes(completeImmediatelyTime);
            var queueTime = GetQueueTime(villageId, contextFactory);

            if (requiredTime > queueTime) return;

            if (!IsSkippableBuilding(villageId, contextFactory)) return;

            await taskManager.Add<CompleteImmediatelyTask>(accountId, villageId);
        }

        private static DateTime GetQueueTime(VillageId villageId, IDbContextFactory<AppDbContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .OrderByDescending(x => x.Position)
                .Select(x => x.CompleteTime)
                .FirstOrDefault();
            return queueBuilding;
        }

        private static bool IsSkippableBuilding(VillageId villageId, IDbContextFactory<AppDbContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();

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

        private static void Clean(VillageId villageId, IDbContextFactory<AppDbContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();
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

        private static int Count(VillageId villageId, IDbContextFactory<AppDbContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();

            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }
    }
}