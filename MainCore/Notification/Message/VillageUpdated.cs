using MainCore.Constraints;
using MainCore.Notification.Handlers.Refresh;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class VillageUpdated
    {
        public sealed record Notification(AccountId AccountId) : IAccountNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            BuildingUpdateTaskTrigger.Handler buildingUpdateTaskTrigger,
            VillageListRefresh.Handler villageListRefresh,
            CancellationToken cancellationToken)
        {
            await buildingUpdateTaskTrigger.HandleAsync(notification, cancellationToken);
            await villageListRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}