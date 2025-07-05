using MainCore.Constraints;

namespace MainCore.Tasks.Base
{
    public abstract record VillageTask(AccountId accountId, VillageId villageId, string villageName) : AccountTask(accountId), IAccountVillageConstraint
    {
        public VillageId VillageId { get; } = villageId;

        protected string VillageName { get; } = villageName;

        public override string Description => $"{TaskName} in {VillageName}";
        public override string Key => $"{AccountId}-{VillageId}";

        public void Deconstruct(out AccountId accountId, out VillageId villageId) => (accountId, villageId) = (AccountId, VillageId);
    }
}