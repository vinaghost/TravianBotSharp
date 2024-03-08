using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class HeroDead : ByAccountIdBase, INotification
    {
        public HeroDead(AccountId accountId) : base(accountId)
        {
        }
    }
}