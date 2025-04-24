using MainCore.Notification.Handlers.Refresh;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class JobUpdated
    {
        public sealed record Notification(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger,
            JobListRefresh.Handler jobListRefresh,
            CancellationToken cancellationToken)
        {
            await upgradeBuildingTaskTrigger.HandleAsync(notification, cancellationToken);
            await jobListRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}