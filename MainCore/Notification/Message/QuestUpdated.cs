using MainCore.Notification.Base;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class QuestUpdated
    {
        public record Notification(AccountId AccountId, VillageId VillageId) : IVillageNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            ClaimQuestTaskTrigger.Handler claimQuestTaskTrigger,
            CancellationToken cancellationToken)
        {
            await claimQuestTaskTrigger.HandleAsync(notification, cancellationToken);
        }
    }
}