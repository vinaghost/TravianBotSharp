using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class ExpansionSlotUpdated : ByAccountVillageIdBase, INotification
    {
        public ExpansionSlotUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}