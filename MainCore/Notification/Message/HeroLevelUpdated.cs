using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class HeroLevelUpdated : ByAccountIdBase, INotification
    {
        public HeroLevelUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}