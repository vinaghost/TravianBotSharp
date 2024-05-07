using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerNPCTask : INotificationHandler<StorageUpdated>, INotificationHandler<VillageSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public TriggerNPCTask(ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory)
        {
            _taskManager = taskManager;
            _contextFactory = contextFactory;
        }

        public async Task Handle(StorageUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var autoNPCEnable = new GetSetting().BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (autoNPCEnable)
            {
                var granaryPercent = GetGranaryPercent(villageId);
                var autoNPCGranaryPercent = new GetSetting().ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

                if (granaryPercent < autoNPCGranaryPercent) return;
                if (_taskManager.IsExist<NPCTask>(accountId, villageId)) return;

                await _taskManager.Add<NPCTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<NPCTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
            }
        }

        private int GetGranaryPercent(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var percent = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Crop * 100f / x.Granary)
                .FirstOrDefault();
            return (int)percent;
        }
    }
}