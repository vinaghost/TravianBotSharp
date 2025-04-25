namespace MainCore.Commands.Queries
{
    [Handler]
    public static partial class GetBuilding
    {
        public sealed record Query(VillageId VillageId, int Location) : ICustomQuery;

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