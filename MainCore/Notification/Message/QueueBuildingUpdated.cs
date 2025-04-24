using MainCore.Notification.Handlers.Refresh;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class QueueBuildingUpdated
    {
        public record Notification(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            CompleteImmediatelyTaskTrigger.Handler completeImmediatelyTaskTrigger,
            QueueRefresh.Handler queueRefresh,
            CancellationToken cancellationToken)
        {
            await completeImmediatelyTaskTrigger.HandleAsync(notification, cancellationToken);
            await queueRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}