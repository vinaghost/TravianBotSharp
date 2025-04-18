namespace MainCore.Notification.Message
{
    public record QueueBuildingUpdated(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;
}