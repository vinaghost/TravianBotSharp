using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class AccountInfoUpdated : ByAccountIdBase, INotification
    {
        public AccountInfoUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}