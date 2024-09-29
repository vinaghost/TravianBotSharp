using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    public class GetBuildingLocation(IDbContextFactory<AppDbContext> contextFactory = null) : QueryBase(contextFactory)
    {
        public int Execute(VillageId villageId, BuildingEnums building)
        {
            using var context = _contextFactory.CreateDbContext();
            var location = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == building)
                .Select(x => x.Location)
                .FirstOrDefault();
            return location;
        }
    }
}