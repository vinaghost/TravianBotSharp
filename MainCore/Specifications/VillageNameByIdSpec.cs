using Ardalis.Specification;

namespace MainCore.Specifications
{
    public class VillageNameByIdSpec : Specification<Village, string>
    {
        public VillageNameByIdSpec(VillageId villageId)
        {
            Query
                .Where(x => x.Id == villageId.Value)
                .Select(x => x.Name);
        }
    }
}