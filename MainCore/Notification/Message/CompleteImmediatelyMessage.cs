using MainCore.Common.MediatR;

namespace MainCore.Notification.Message
{
    public class CompleteImmediatelyMessage : ByAccountVillageIdBase, INotification
    {
        public CompleteImmediatelyMessage(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}