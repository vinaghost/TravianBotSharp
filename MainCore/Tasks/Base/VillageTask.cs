using MainCore.Tasks.Constraints;

namespace MainCore.Tasks.Base
{
    public abstract class VillageTask(AccountId accountId, VillageId villageId, string villageName, DateTime executeAt) : AccountTask(accountId, executeAt), IVillageTask

    {
        public VillageId VillageId { get; } = villageId;

        protected string VillageName { get; } = villageName;

        public override string Description => $"{TaskName} in {VillageName}";
    }
}