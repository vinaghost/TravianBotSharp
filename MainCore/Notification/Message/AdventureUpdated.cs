namespace MainCore.Notification.Message
{
    public record AdventureUpdated(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}