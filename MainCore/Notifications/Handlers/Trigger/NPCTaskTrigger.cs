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
            if (autoNPCEnable)
            {
                var granaryPercent = context.GetGranaryPercent(villageId);
                var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

                if (granaryPercent < autoNPCGranaryPercent) return;
                if (taskManager.IsExist<NpcTask.Task>(accountId, villageId)) return;
                var villageName = context.GetVillageName(villageId);
                taskManager.Add<NpcTask.Task>(new(accountId, villageId, villageName));
            }
            else
            {
                taskManager.Remove<NpcTask.Task>(accountId);
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