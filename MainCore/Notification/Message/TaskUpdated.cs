namespace MainCore.Notification.Message
{
    public class TaskUpdated : ByAccountIdBase, INotification
    {
        public TaskUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}