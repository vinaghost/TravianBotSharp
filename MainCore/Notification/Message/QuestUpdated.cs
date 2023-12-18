using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class QuestUpdated : ByAccountVillageIdBase, INotification
    {
        public QuestUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}