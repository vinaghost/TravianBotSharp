using MainCore.Entities;

namespace MainCore.Common.MediatR
{
    public class ByAccountVillageIdBase : ByAccountIdBase
    {
        public VillageId VillageId { get; }

        public ByAccountVillageIdBase(AccountId accountId, VillageId villageId) : base(accountId)
        {
            VillageId = villageId;
        }
    }
}