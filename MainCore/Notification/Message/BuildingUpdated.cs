namespace MainCore.Notification.Message
{
    public record BuildingUpdated(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;
}