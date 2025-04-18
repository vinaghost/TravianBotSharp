namespace MainCore.Notification.Message
{
    public record CompleteImmediatelyMessage(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;
}