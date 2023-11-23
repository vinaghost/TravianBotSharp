using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class BuildingUpdated : ByAccountVillageIdBase, INotification
    {
        public BuildingUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}