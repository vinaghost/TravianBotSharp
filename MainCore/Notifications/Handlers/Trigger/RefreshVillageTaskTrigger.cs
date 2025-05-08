using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class RefreshVillageTaskTrigger
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

            var autoRefreshEnable = settingService.BooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
            if (autoRefreshEnable)
            {
                if (taskManager.IsExist<UpdateVillageTask.Task>(accountId, villageId)) return;
                var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
                await taskManager.Add<UpdateVillageTask.Task>(new(accountId, villageId, villageName));
            }
            else
            {
                var task = taskManager.Get<UpdateVillageTask.Task>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}