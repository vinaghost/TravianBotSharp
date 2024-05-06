namespace MainCore.Notification.Message
{
    public class QueueBuildingUpdated : ByAccountVillageIdBase, INotification
    {
        public QueueBuildingUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}