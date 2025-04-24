using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class CompleteImmediatelyMessage
    {
        public sealed record Notification(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger,
            CancellationToken cancellationToken)
        {
            await upgradeBuildingTaskTrigger.HandleAsync(notification, cancellationToken);
        }
    }
}