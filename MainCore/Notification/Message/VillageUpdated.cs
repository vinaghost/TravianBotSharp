using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class VillageUpdated
    {
        public sealed record Notification(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            BuildingUpdateTaskTrigger.Handler buildingUpdateTaskTrigger,
            CancellationToken cancellationToken)
        {
            await buildingUpdateTaskTrigger.HandleAsync(notification, cancellationToken);
        }
    }
}