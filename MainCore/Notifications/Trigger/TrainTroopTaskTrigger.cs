using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class TrainTroopTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            ITaskManager taskManager,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (accountId, villageId) = notification;

            var taskExist = taskManager.IsExist<TrainTroopTask.Task>(accountId, villageId);
            if (taskExist) return;

            var settingEnable = context.BooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
            if (!settingEnable) return;

            var villageName = context.GetVillageName(villageId);
            taskManager.Add<TrainTroopTask.Task>(new(accountId, villageId, villageName));
        }
    }
}