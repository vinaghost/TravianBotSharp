using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class NpcTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountVillageIdBase notification,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            var autoNPCEnable = getSetting.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (autoNPCEnable)
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var granaryPercent = GetGranaryPercent(context, villageId);
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

        private static int GetGranaryPercent(AppDbContext context, VillageId villageId)
        {
            var percent = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Crop * 100f / x.Granary)
                .FirstOrDefault();
            return (int)percent;
        }
    }
}