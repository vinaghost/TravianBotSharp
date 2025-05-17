using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetFirstQueueBuildingQuery
    {
        public sealed record Query(VillageId VillageId) : IVillageQuery;

        private static async ValueTask<QueueBuilding?> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var villageId = query.VillageId;
            var queueBuilding = context.QueueBuildings
               .Where(x => x.VillageId == villageId.Value)
               .Where(x => x.Type != BuildingEnums.Site)
               .OrderBy(x => x.Position)
               .FirstOrDefault();
            return queueBuilding;
        }
    }
}