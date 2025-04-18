namespace MainCore.Notification.Message
{
    public record FarmListUpdated(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}