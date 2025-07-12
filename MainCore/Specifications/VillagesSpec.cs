using Ardalis.Specification;

namespace MainCore.Specifications
{
    public class VillagesSpec : Specification<Village, VillageId>
    {
        public VillagesSpec(AccountId accountId)
        {
            Query
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new VillageId(x.Id));
        }
    }
}