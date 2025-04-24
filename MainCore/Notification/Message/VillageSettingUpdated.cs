namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class ChangeWallTrigger
    {
        public sealed record Notification(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            ChangeWallTrigger.Handler changeWallTrigger,
            CancellationToken cancellationToken)
        {
            await changeWallTrigger.HandleAsync(notification, cancellationToken);
        }
    }
}