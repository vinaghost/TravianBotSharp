namespace MainCore.Notification.Message
{
    public record HeroItemUpdated(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;
}