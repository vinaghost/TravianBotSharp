using MainCore.Constraints;
using MainCore.Entities;
using MainCore.Enums;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetQueueBuildingByTypeQuery
    {
        public sealed record Query(VillageId VillageId, BuildingEnums BuildingType) : IVillageQuery;

        private static async ValueTask<QueueBuilding?> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (villageId, buildingType) = query;

            var queueBuilding = context.QueueBuildings
               .Where(x => x.VillageId == villageId.Value)
               .Where(x => x.Type == buildingType)
               .OrderBy(x => x.Position)
               .FirstOrDefault();

            return queueBuilding;
        }
    }
}
