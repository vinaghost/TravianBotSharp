using MainCore.Notification.Base;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class ClaimQuestTaskTrigger
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
            var autoClaimQuest = context.BooleanByName(villageId, VillageSettingEnums.AutoClaimQuestEnable);
            if (autoClaimQuest)
            {
                if (taskManager.IsExist<ClaimQuestTask.Task>(accountId, villageId)) return;
                var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
                await taskManager.Add<ClaimQuestTask.Task>(new(accountId, villageId, villageName));
            }
            else
            {
                var task = taskManager.Get<ClaimQuestTask.Task>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}