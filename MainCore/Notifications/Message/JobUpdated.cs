using MainCore.Constraints;
using MainCore.Notifications.Handlers.Refresh;
using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Notifications.Message
{
    [Handler]
    public static partial class JobUpdated
    {
        public sealed record Notification(AccountId AccountId, VillageId VillageId) : IAccountVillageNotification;

        private static async ValueTask HandleAsync(
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