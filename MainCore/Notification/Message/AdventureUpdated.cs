namespace MainCore.Notification.Message
{
    public class AdventureUpdated : ByAccountIdBase, INotification
    {
        public AdventureUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}