namespace MainCore.Notification.Message
{
    public record AccountInfoUpdated(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}