using MainCore.Queries.Base;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetBuildingQuery
    {
        public sealed record Query(VillageId VillageId, int Location) : IQuery;

        private static async ValueTask<BuildingDto> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken token
        )
        {
            var (villageId, location) = query;
            using var context = await contextFactory.CreateDbContextAsync();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .ToDto()
                .FirstOrDefault();
            return building;
        }
    }
}