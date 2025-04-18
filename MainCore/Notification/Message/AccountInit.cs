namespace MainCore.Notification.Message
{
    public record AccountInit(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}