using MainCore.Notification.Base;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class TrainTroopTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IVillageNotification notification,
            GetVillageNameQuery.Handler getVillageNameQuery,
            ITaskManager taskManager,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            using var context = await contextFactory.CreateDbContextAsync();
            var trainTroopEnable = context.BooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
            if (trainTroopEnable)
            {
                if (taskManager.IsExist<TrainTroopTask.Task>(accountId, villageId)) return;
                var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
                await taskManager.Add<TrainTroopTask.Task>(new(accountId, villageId, villageName));
            }
            else
            {
                var task = taskManager.Get<TrainTroopTask.Task>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}