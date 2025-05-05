using MainCore.Notification.Base;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class RefreshVillageTaskTrigger
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
            using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var autoRefreshEnable = context.BooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
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