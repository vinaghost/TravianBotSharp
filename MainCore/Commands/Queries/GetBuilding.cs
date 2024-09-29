using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    public class GetBuilding(IDbContextFactory<AppDbContext> contextFactory = null) : QueryBase(contextFactory)
    {
        public BuildingDto Execute(VillageId villageId, int location)
        {
            using var context = _contextFactory.CreateDbContext();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .ToDto()
                .FirstOrDefault();
            return building;
        }
    }
}