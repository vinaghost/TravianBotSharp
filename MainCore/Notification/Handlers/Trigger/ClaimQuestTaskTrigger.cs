using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class ClaimQuestTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountVillageIdBase notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            var autoClaimQuest = getSetting.BooleanByName(villageId, VillageSettingEnums.AutoClaimQuestEnable);
            if (autoClaimQuest)
            {
                if (taskManager.IsExist<ClaimQuestTask>(accountId, villageId)) return;
                await taskManager.Add<ClaimQuestTask>(accountId, villageId);
            }
            else
            {
                var task = taskManager.Get<ClaimQuestTask>(accountId, villageId);
                await taskManager.Remove(accountId, task);
            }
        }
    }
}