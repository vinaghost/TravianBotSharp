namespace MainCore.Notification.Message
{
    public record JobUpdated(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;
}