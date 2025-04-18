namespace MainCore.Notification.Message
{
    public record AccountSettingUpdated(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}