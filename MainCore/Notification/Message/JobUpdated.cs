using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class JobUpdated : ByAccountVillageIdBase, INotification
    {
        public JobUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}