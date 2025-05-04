using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class ClaimQuestTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountVillageIdBase notification,
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
                await taskManager.Add<ClaimQuestTask.Task>(accountId, villageId);
            }
            else
            {
                var task = taskManager.Get<ClaimQuestTask.Task>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}