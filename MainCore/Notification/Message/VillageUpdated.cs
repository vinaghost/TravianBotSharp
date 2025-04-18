namespace MainCore.Notification.Message
{
    public record VillageUpdated(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}