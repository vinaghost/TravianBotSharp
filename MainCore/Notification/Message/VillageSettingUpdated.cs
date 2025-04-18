namespace MainCore.Notification.Message
{
    public record VillageSettingUpdated(AccountId AccountId, VillageId VillageId) : ByAccountVillageIdBase(AccountId, VillageId), INotification;
}