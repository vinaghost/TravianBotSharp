using MainCore.Constraints;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class ClaimQuestTaskTrigger
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

            var autoClaimQuest = settingService.BooleanByName(villageId, VillageSettingEnums.AutoClaimQuestEnable);
            if (autoClaimQuest)
            {
                if (taskManager.IsExist<ClaimQuestTask.Task>(accountId, villageId)) return;
                var villageName = await getVillageNameQuery.HandleAsync(new(villageId), cancellationToken);
                taskManager.Add<ClaimQuestTask.Task>(new(accountId, villageId, villageName));
            }
            else
            {
                taskManager.Remove<ClaimQuestTask.Task>(accountId);
            }
        }
    }
}