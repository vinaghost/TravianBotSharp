using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class AttackFound : ByAccountVillageIdBase, INotification
    {
        public AttackFound(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}