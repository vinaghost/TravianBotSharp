using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class AccountLogout : ByAccountIdBase, INotification
    {
        public AccountLogout(AccountId accountId) : base(accountId)
        {
        }
    }
}