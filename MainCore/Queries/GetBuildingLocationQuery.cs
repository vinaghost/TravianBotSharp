using MainCore.Queries.Base;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetBuildingLocationQuery
    {
        public sealed record Query(VillageId VillageId, BuildingEnums Building) : IQuery;

        private static async ValueTask<int> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var (villageId, building) = query;
            using var context = await contextFactory.CreateDbContextAsync();

            var location = context.Buildings
                 .Where(x => x.VillageId == villageId.Value)
                 .Where(x => x.Type == building)
                 .Select(x => x.Location)
                 .FirstOrDefault();
            return location;
        }
    }
}