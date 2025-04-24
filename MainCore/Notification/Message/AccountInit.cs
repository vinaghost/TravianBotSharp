using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class AccountInit
    {
        public sealed record Notification(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            LoginTaskTrigger.Handler loginTaskTrigger,
            GetVillage getVillage,
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
            var villages = getVillage.All(accountId);

            foreach (var village in villages)
            {
                await refreshVillageTaskTrigger.HandleAsync(new(accountId, village), cancellationToken);
                await trainTroopTaskTrigger.HandleAsync(new(accountId, village), cancellationToken);
            }

            var hasBuildingJobVillages = getVillage.HasBuildingJob(accountId);
            foreach (var village in hasBuildingJobVillages)
            {
                await upgradeBuildingTaskTrigger.HandleAsync(new(accountId, village), cancellationToken);
            }
        }
    }
}