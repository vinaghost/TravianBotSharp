using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class AccountInit : ByAccountIdBase, INotification
    {
        public AccountInit(AccountId accountId) : base(accountId)
        {
        }
    }
}