namespace MainCore.Notification.Message
{
    public record TaskUpdated(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}