using MainCore.Constraints;

using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Notifications.Message
{
    [Handler]
    public static partial class AccountInit
    {
        public sealed record Notification(AccountId AccountId) : IAccountNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            GetVillagesQuery.Handler getVillagesQuery,
            GetHasBuildJobVillagesQuery.Handler getHasBuildJobVillagesQuery,
            LoginTaskTrigger.Handler loginTaskTrigger,
            RefreshVillageTaskTrigger.Handler refreshVillageTaskTrigger,
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
            var villages = await getVillagesQuery.HandleAsync(new(accountId));

            foreach (var village in villages)
            {
                await refreshVillageTaskTrigger.HandleAsync(new VillageNotification(accountId, village), cancellationToken);
                await trainTroopTaskTrigger.HandleAsync(new VillageNotification(accountId, village), cancellationToken);
            }

            var hasBuildingJobVillages = await getHasBuildJobVillagesQuery.HandleAsync(new(accountId));
            foreach (var village in hasBuildingJobVillages)
            {
                await upgradeBuildingTaskTrigger.HandleAsync(new VillageNotification(accountId, village), cancellationToken);
            }
        }
    }
}