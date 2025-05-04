using MainCore.Tasks.Constraints;

namespace MainCore.Tasks.Base
{
    public abstract class VillageTask(AccountId accountId, VillageId villageId, string villageName) : AccountTask(accountId), IVillageTask
    {
        public VillageId VillageId { get; } = villageId;

        protected string VillageName { get; } = villageName;

        public override string Description => $"{TaskName} in {VillageName}";
        public override string Key => $"{AccountId}-{VillageId}";
    }
}