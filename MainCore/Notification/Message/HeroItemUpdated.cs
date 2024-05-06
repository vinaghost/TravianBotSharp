namespace MainCore.Notification.Message
{
    public class HeroItemUpdated : ByAccountIdBase, INotification
    {
        public HeroItemUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}