namespace MainCore.Notification.Message
{
    public record StorageUpdated(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;
}