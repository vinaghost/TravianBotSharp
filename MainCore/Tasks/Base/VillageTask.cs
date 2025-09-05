namespace MainCore.Tasks.Base
{
    public abstract class VillageTask(AccountId accountId, VillageId villageId) : AccountTask(accountId), IAccountVillageConstraint
    {
        public VillageId VillageId { get; } = villageId;

        protected string VillageName { get; set; } = "Unknown village";

        public void SetVillageName(AppDbContext context)
        {
            if (VillageName != "Unknown village")
                return;

            var getVillageNameSpec = new GetVillageNameSpec(VillageId);
            VillageName = context.Villages
                .WithSpecification(getVillageNameSpec)
                .FirstOrDefault() ?? "Unknown village";
        }

        public override string Description => $"{TaskName} in {VillageName}";
        public override string Key => $"{AccountId}-{VillageId}";

        public void Deconstruct(out AccountId accountId, out VillageId villageId) => (accountId, villageId) = (AccountId, VillageId);
    }
}
