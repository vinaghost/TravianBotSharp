using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class AccountSettingUpdated : ByAccountIdBase, INotification
    {
        public AccountSettingUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}