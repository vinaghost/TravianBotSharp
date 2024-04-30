using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class VillageUpdated : ByAccountIdBase, INotification
    {
        public VillageUpdated(AccountId accountId) : base(accountId)
        {
        }
    }
}