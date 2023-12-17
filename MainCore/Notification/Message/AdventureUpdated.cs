using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class AdventureUpdated : ByAccountIdBase, INotification
    {
        public AdventureUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}