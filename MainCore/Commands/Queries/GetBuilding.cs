using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetBuilding>]
    public class GetBuilding(IDbContextFactory<AppDbContext> contextFactory) : QueryBase(contextFactory)
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