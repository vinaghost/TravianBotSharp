using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class RefreshVillageTaskTrigger
    {
        private static async ValueTask HandleAsync(
            VillageSettingUpdated notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            GetVillage getVillage,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId, taskManager, getSetting);
        }

        private static async ValueTask HandleAsync(
            AccountInit notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            GetVillage getVillage,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villages = getVillage.All(accountId);

            foreach (var village in villages)
            {
                await Trigger(accountId, village, taskManager, getSetting);
            }
        }

        private static async Task Trigger(
            AccountId accountId,
            VillageId villageId,
            ITaskManager taskManager,
            IGetSetting getSetting)
        {
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