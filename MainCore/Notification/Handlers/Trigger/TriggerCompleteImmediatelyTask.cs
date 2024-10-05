using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerCompleteImmediatelyTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<QueueBuildingUpdated>
    {
        private readonly ITaskManager _taskManager;

        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly GetSetting _getSetting;

        public TriggerCompleteImmediatelyTask(ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory, GetSetting getSetting)
        {
            _taskManager = taskManager;
            _contextFactory = contextFactory;
            _getSetting = getSetting;
        }

        public async Task Handle(QueueBuildingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            if (_taskManager.IsExist<CompleteImmediatelyTask>(accountId, villageId)) return;
            Clean(villageId);

            var count = Count(villageId);
            if (count == 0) return;

            var completeImmediatelyEnable = _getSetting.BooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
            if (!completeImmediatelyEnable) return;

            if (!IsSkippableBuilding(villageId)) return;

            var completeImmediatelyTime = _getSetting.ByName(villageId, VillageSettingEnums.CompleteImmediatelyTime);

            var requiredTime = DateTime.Now.AddMinutes(completeImmediatelyTime);
            var queueTime = GetQueueTime(villageId);

            if (requiredTime > queueTime) return;

            if (!IsSkippableBuilding(villageId)) return;

            await _taskManager.Add<CompleteImmediatelyTask>(accountId, villageId);
        }

        private DateTime GetQueueTime(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .OrderByDescending(x => x.Position)
                .Select(x => x.CompleteTime)
                .FirstOrDefault();
            return queueBuilding;
        }

        private bool IsSkippableBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var buildings = new List<BuildingEnums>()
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

        private void Clean(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
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

        private int Count(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }
    }
}