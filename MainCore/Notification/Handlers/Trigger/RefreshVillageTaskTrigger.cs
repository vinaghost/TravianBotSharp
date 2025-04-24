using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class RefreshVillageTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountVillageIdBase notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            var autoRefreshEnable = getSetting.BooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
            if (autoRefreshEnable)
            {
                if (taskManager.IsExist<UpdateVillageTask>(accountId, villageId)) return;
                await taskManager.Add<UpdateVillageTask>(accountId, villageId);
            }
            else
            {
                var task = taskManager.Get<UpdateVillageTask>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}