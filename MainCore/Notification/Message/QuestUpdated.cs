namespace MainCore.Notification.Message
{
    public record QuestUpdated(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;
}