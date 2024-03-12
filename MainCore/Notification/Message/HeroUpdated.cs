using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class HeroUpdated : ByAccountIdBase, INotification
    {
        public HeroUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}