using MainCore.Notification.Base;
using MainCore.Notification.Handlers.Refresh;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class BuildingUpdated
    {
        public sealed record Notification(AccountId AccountId, VillageId VillageId) : IVillageNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            BuildingListRefresh.Handler buildingListRefresh,
            CancellationToken cancellationToken)
        {
            await buildingListRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}