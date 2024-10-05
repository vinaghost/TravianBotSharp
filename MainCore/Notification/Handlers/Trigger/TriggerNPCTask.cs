using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerNpcTask : INotificationHandler<StorageUpdated>, INotificationHandler<VillageSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly GetSetting _getSetting;

        public TriggerNpcTask(ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory, GetSetting getSetting)
        {
            _taskManager = taskManager;
            _contextFactory = contextFactory;
            _getSetting = getSetting;
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
            var autoNPCEnable = _getSetting.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (autoNPCEnable)
            {
                var granaryPercent = GetGranaryPercent(villageId);
                var autoNPCGranaryPercent = _getSetting.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

                if (granaryPercent < autoNPCGranaryPercent) return;
                if (_taskManager.IsExist<NpcTask>(accountId, villageId)) return;

                await _taskManager.Add<NpcTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<NpcTask>(accountId, villageId);
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