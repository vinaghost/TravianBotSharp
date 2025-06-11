using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class NpcTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            ITaskManager taskManager,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;

            var autoNPCEnable = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (!autoNPCEnable)
            {
                taskManager.Remove<NpcTask.Task>(accountId, villageId);
                return;
            }

            var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);
            var (crop, granary, production) = context.GetCropInfo(villageId);
            if (granary == 0) return;

            var currentPercent = crop * 100f / granary;
            var villageName = context.GetVillageName(villageId);
            var npcTask = new NpcTask.Task(accountId, villageId, villageName);

            if (currentPercent >= autoNPCGranaryPercent)
            {
                if (taskManager.IsExist<NpcTask.Task>(accountId, villageId)) return;
                taskManager.Add<NpcTask.Task>(npcTask);
            }
            else if (production > 0)
            {
                var targetCrop = granary * autoNPCGranaryPercent / 100.0;
                var hours = (targetCrop - crop) / production;
                npcTask.ExecuteAt = DateTime.Now.AddHours(hours);
                taskManager.AddOrUpdate<NpcTask.Task>(npcTask);
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

        private static (long Crop, long Granary, long Production) GetCropInfo(this AppDbContext context, VillageId villageId)
        {
            var data = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => new { x.Crop, x.Granary, x.ProductionCrop })
                .FirstOrDefault();
            if (data is null) return (0, 0, 0);
            return (data.Crop, data.Granary, data.ProductionCrop);
        }
    }
}