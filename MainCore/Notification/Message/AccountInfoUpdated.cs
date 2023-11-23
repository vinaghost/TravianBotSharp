using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class AccountInfoUpdated : ByAccountIdBase, INotification
    {
        public AccountInfoUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}