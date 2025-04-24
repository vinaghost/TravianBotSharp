using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class NpcTaskTrigger
    {
        private static async ValueTask HandleAsync(
            StorageUpdated notification,
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
            var autoNPCEnable = getSetting.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (autoNPCEnable)
            {
                var granaryPercent = GetGranaryPercent(villageId, contextFactory);
                var autoNPCGranaryPercent = getSetting.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

                if (granaryPercent < autoNPCGranaryPercent) return;
                if (taskManager.IsExist<NpcTask>(accountId, villageId)) return;

                await taskManager.Add<NpcTask>(accountId, villageId);
            }
            else
            {
                var task = taskManager.Get<NpcTask>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }

        private static int GetGranaryPercent(VillageId villageId, IDbContextFactory<AppDbContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();
            var percent = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Crop * 100f / x.Granary)
                .FirstOrDefault();
            return (int)percent;
        }
    }
}