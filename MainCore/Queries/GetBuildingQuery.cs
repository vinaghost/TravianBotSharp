using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetBuildingQuery
    {
        public sealed record Query(VillageId VillageId, int Location) : IVillageQuery;

        private static async ValueTask<BuildingDto> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken token
        )
        {
            await Task.CompletedTask;
            var (villageId, location) = query;

            var building = context.GetBuilding(villageId, location);
            return building;
        }

        public static BuildingDto GetBuilding(this AppDbContext context, VillageId villageId, int location)
        {
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .ToDto()
                .FirstOrDefault();
            return building;
        }
    }
}