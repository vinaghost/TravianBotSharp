using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class BuildingUpdated : ByAccountVillageIdBase, INotification
    {
        public BuildingUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}