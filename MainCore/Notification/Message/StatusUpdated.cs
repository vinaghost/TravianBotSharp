namespace MainCore.Notification.Message
{
    public record StatusUpdated(AccountId AccountId, StatusEnums Status) : ByAccountIdBase(AccountId), INotification;
}