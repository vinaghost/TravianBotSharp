using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class TrainTroopTaskTrigger
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
            var villages = getVillage.All(notification.AccountId);
            foreach (var village in villages)
            {
                await Trigger(notification.AccountId, village, taskManager, getSetting);
            }
        }

        private static async Task Trigger(
            AccountId accountId,
            VillageId villageId,
            ITaskManager taskManager,
            IGetSetting getSetting)
        {
            var trainTroopEnable = getSetting.BooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
            if (trainTroopEnable)
            {
                if (taskManager.IsExist<TrainTroopTask>(accountId, villageId)) return;
                await taskManager.Add<TrainTroopTask>(accountId, villageId);
            }
            else
            {
                var task = taskManager.Get<TrainTroopTask>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}