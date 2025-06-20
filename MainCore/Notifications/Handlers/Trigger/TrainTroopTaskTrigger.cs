using MainCore.Constraints;
using MainCore.Commands.Features.TrainTroop;
using MainCore.Commands.NextExecute;
using MainCore.Enums;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class TrainTroopTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            GetTrainQueueTimeCommand.Handler getTrainQueueTimeCommand,
            NextExecuteTrainTroopTaskCommand.Handler nextExecuteTrainTroopTaskCommand,
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
                var task = new TrainTroopTask.Task(accountId, villageId, villageName);

                var queueResult = await getTrainQueueTimeCommand.HandleAsync(new(accountId, villageId, BuildingEnums.Barracks), cancellationToken);
                var queueTime = queueResult.IsFailed ? TimeSpan.Zero : queueResult.Value;
                await nextExecuteTrainTroopTaskCommand.HandleAsync(new(task, queueTime), cancellationToken);

                taskManager.Add<TrainTroopTask.Task>(task);
            }
            else
            {
                taskManager.Remove<TrainTroopTask.Task>(accountId, villageId);
            }
        }
    }
}