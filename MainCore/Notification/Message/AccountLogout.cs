using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class AccountLogout : ByAccountIdBase, INotification
    {
        public AccountLogout(AccountId accountId) : base(accountId)
        {
        }
    }
}