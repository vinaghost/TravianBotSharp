using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class VillageUpdated : ByAccountIdBase, INotification
    {
        public VillageUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}