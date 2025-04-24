using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class TrainTroopTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountVillageIdBase notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
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