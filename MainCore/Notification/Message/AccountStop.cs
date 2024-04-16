using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class AccountStop : ByAccountIdBase, INotification
    {
        public AccountStop(AccountId accountId) : base(accountId)
        {
        }
    }
}