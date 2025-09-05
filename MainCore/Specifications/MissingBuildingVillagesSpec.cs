using Ardalis.Specification;

namespace MainCore.Specifications
{
    public class MissingBuildingVillagesSpec : Specification<Village, VillageId>
    {
        public MissingBuildingVillagesSpec(AccountId accountId)
        {
            Query
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Count != 40)
                .Select(x => new VillageId(x.Id));
        }
    }
}
