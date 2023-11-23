using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class FarmListUpdated : ByAccountIdBase, INotification
    {
        public FarmListUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}