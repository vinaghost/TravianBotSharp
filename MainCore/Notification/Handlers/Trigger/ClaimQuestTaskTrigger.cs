using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class ClaimQuestTaskTrigger
    {
        private static async ValueTask HandleAsync(
            QuestUpdated notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId, taskManager, getSetting);
        }

        private static async ValueTask HandleAsync(
            VillageSettingUpdated notification,
            ITaskManager taskManager,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId, taskManager, getSetting);
        }

        private static async Task Trigger(
            AccountId accountId,
            VillageId villageId,
            ITaskManager taskManager,
            IGetSetting getSetting)
        {
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