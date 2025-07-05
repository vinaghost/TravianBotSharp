using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class TrainTroopTaskTrigger
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

            var trainTroopEnable = settingService.BooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
            if (trainTroopEnable)
            {
                if (taskManager.IsExist<TrainTroopTask.Task>(accountId, villageId)) return;
                var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
                taskManager.Add<TrainTroopTask.Task>(new(accountId, villageId, villageName));
            }
            else
            {
                taskManager.Remove<TrainTroopTask.Task>(accountId);
            }
        }
    }
}