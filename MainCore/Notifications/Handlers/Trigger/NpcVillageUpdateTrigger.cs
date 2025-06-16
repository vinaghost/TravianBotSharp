using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class NpcVillageUpdateTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;

            var autoNpcEnable = settingService.BooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (!autoNpcEnable) return;

            var hasNpcTask = taskManager
                .GetTaskList(accountId)
                .OfType<NpcTask.Task>()
                .Any(x => x.VillageId == villageId);
            if (hasNpcTask) return;

            if (taskManager.IsExist<UpdateVillageTask.Task>(accountId, villageId)) return;

            var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
            taskManager.Add<UpdateVillageTask.Task>(new(accountId, villageId, villageName));
        }
    }
}

