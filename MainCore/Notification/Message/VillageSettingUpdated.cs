using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class VillageSettingUpdated : ByAccountVillageIdBase, INotification
    {
        public VillageSettingUpdated(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }
}