using Ardalis.Specification;

namespace MainCore.Specifications
{
    public class GetBuildingSpec : Specification<Building>
    {
        public GetBuildingSpec(VillageId villageId, int location)
        {
            Query
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location);
        }
    }
}
