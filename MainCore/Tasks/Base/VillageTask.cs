using MainCore.Entities;

namespace MainCore.Tasks.Base
{
    public abstract class VillageTask : TaskBase
    {
        public AccountId AccountId { get; private set; }
        public VillageId VillageId { get; private set; }

        public void Setup(AccountId accountId, VillageId villageId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            VillageId = villageId;
            CancellationToken = cancellationToken;
        }
    }
}