using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetLowestBuildingQuery
    {
        public sealed record Query(VillageId VillageId, BuildingEnums BuildingType) : IQuery;

        private static async ValueTask<Building> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (villageId, buildingType) = query;

            var building = context.Buildings
                 .Where(x => x.VillageId == villageId.Value)
                 .Where(x => x.Type == buildingType)
                 .OrderBy(x => x.Level)
                 .FirstOrDefault();
            return building;
        }
    }
}