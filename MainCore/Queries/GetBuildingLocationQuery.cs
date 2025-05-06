using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetBuildingLocationQuery
    {
        public sealed record Query(VillageId VillageId, BuildingEnums Building) : IQuery;

        private static async ValueTask<int> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (villageId, building) = query;

            var location = context.Buildings
                 .Where(x => x.VillageId == villageId.Value)
                 .Where(x => x.Type == building)
                 .Select(x => x.Location)
                 .FirstOrDefault();
            return location;
        }
    }
}