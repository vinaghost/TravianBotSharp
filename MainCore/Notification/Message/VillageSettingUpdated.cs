using MainCore.Notification.Handlers.Refresh;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class VillageSettingUpdated
    {
        public sealed record Notification(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            ChangeWallTrigger.Handler changeWallTrigger,
            ClaimQuestTaskTrigger.Handler claimQuestTaskTrigger,
            CompleteImmediatelyTaskTrigger.Handler completeImmediatelyTaskTrigger,
            NpcTaskTrigger.Handler npcTaskTrigger,
            RefreshVillageTaskTrigger.Handler refreshVillageTaskTrigger,
            TrainTroopTaskTrigger.Handler trainTroopTaskTrigger,
            VillageSettingRefresh.Handler villageSettingRefresh,
            CancellationToken cancellationToken)
        {
            await changeWallTrigger.HandleAsync(notification, cancellationToken);
            await claimQuestTaskTrigger.HandleAsync(notification, cancellationToken);
            await completeImmediatelyTaskTrigger.HandleAsync(notification, cancellationToken);
            await npcTaskTrigger.HandleAsync(notification, cancellationToken);
            await refreshVillageTaskTrigger.HandleAsync(notification, cancellationToken);
            await trainTroopTaskTrigger.HandleAsync(notification, cancellationToken);
            await villageSettingRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}