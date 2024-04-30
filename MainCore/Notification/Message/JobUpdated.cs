namespace MainCore.Notification.Message
{
    public class JobUpdated : ByAccountVillageIdBase, INotification
    {
        public JobUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}