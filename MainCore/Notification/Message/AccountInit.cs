using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class AccountInit : ByAccountIdBase, INotification
    {
        public AccountInit(AccountId accountId) : base(accountId)
        {
        }
    }
}