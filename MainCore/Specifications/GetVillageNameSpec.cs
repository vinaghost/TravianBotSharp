using Ardalis.Specification;

namespace MainCore.Specifications
{
    public class GetVillageNameSpec : Specification<Village, string>
    {
        public GetVillageNameSpec(VillageId villageId)
        {
            Query
                .Where(x => x.Id == villageId.Value)
                .Select(x => x.Name);
        }
    }
}