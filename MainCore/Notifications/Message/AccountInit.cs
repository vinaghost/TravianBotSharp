using MainCore.Constraints;

using MainCore.Notifications.Trigger;
using MainCore.Specifications;

namespace MainCore.Notifications.Message
{
    [Handler]
    public static partial class AccountInit
    {
        public sealed record Notification(AccountId AccountId) : IAccountNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            AppDbContext context,
            LoginTaskTrigger.Handler loginTaskTrigger,
            UpdateVillageTaskTrigger.Handler refreshVillageTaskTrigger,
            SleepTaskTrigger.Handler sleepTaskTrigger,
            StartAdventureTaskTrigger.Handler startAdventureTaskTrigger,
            TrainTroopTaskTrigger.Handler trainTroopTaskTrigger,
            UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger,
            CancellationToken cancellationToken)
        {
            await loginTaskTrigger.HandleAsync(notification, cancellationToken);

            await sleepTaskTrigger.HandleAsync(notification, cancellationToken);
            await startAdventureTaskTrigger.HandleAsync(notification, cancellationToken);

            var accountId = notification.AccountId;

            var villagesSpec = new VillagesSpec(accountId);
            var villages = context.Villages
                .WithSpecification(villagesSpec)
                .ToList();

            foreach (var village in villages)
            {
                await refreshVillageTaskTrigger.HandleAsync(new VillageNotification(accountId, village), cancellationToken);
                await trainTroopTaskTrigger.HandleAsync(new VillageNotification(accountId, village), cancellationToken);
            }

            var hasBuildJobVillagesSpec = new HasBuildJobVillagesSpec(accountId);
            var hasBuildJobVillages = context.Villages
                .WithSpecification(hasBuildJobVillagesSpec)
                .ToList();
            foreach (var village in hasBuildJobVillages)
            {
                await upgradeBuildingTaskTrigger.HandleAsync(new VillageNotification(accountId, village), cancellationToken);
            }
        }
    }
}