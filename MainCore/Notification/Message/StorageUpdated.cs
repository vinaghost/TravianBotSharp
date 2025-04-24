using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class StorageUpdated
    {
        public sealed record Notification(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            NpcTaskTrigger.Handler npcTaskTrigger,
            CancellationToken cancellationToken)
        {
            await npcTaskTrigger.HandleAsync(notification, cancellationToken);
        }
    }
}