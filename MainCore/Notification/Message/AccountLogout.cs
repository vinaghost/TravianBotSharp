namespace MainCore.Notification.Message
{
    public record AccountLogout(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}