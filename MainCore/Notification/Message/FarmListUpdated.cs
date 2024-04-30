using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class FarmListUpdated : ByAccountIdBase, INotification
    {
        public FarmListUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}