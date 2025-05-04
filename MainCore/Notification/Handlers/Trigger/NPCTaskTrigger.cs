using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class NpcTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountVillageIdBase notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            using var context = await contextFactory.CreateDbContextAsync();
            var autoNPCEnable = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (autoNPCEnable)
            {
                var granaryPercent = context.GetGranaryPercent(villageId);
                var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

                if (granaryPercent < autoNPCGranaryPercent) return;
                if (taskManager.IsExist<NpcTask.Task>(accountId, villageId)) return;
                var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
                await taskManager.Add<NpcTask.Task>(new(accountId, villageId, villageName));
            }
            else
            {
                var task = taskManager.Get<NpcTask.Task>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }

        private static int GetGranaryPercent(this AppDbContext context, VillageId villageId)
        {
            var percent = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Crop * 100f / x.Granary)
                .FirstOrDefault();
            return (int)percent;
        }
    }
}