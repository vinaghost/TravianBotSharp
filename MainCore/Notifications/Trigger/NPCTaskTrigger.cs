using MainCore.Constraints;
using MainCore.Specifications;

namespace MainCore.Notifications.Trigger
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
            var (accountId, villageId) = notification;

            var taskExist = taskManager.IsExist<NpcTask.Task>(accountId, villageId);
            if (taskExist) return;

            var settingEnable = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (!settingEnable) return;

            var gold = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.Gold)
                .FirstOrDefault();
            if (gold < 3) return;

            var granaryPercent = (int)context.Storages
               .Where(x => x.VillageId == villageId.Value)
               .Select(x => x.Crop * 100f / x.Granary)
               .FirstOrDefault();

            var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);
            if (granaryPercent < autoNPCGranaryPercent) return;

            var getVillageSpec = new GetVillageNameSpec(villageId);
            var villageName = context.Villages
                .WithSpecification(getVillageSpec)
                .First();
            taskManager.Add<NpcTask.Task>(new(accountId, villageId, villageName));
        }
    }
}