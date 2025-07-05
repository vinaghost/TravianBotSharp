using MainCore.Constraints;
using MainCore.Specifications;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetBuildingQuery
    {
        public sealed record Query(VillageId VillageId, int Location) : IVillageQuery;

        private static async ValueTask<BuildingDto> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (villageId, location) = query;

            var spec = new GetBuildingSpec(villageId, location);
            var building = context.Buildings
                .WithSpecification(spec)
                .ToDto()
                .First();
            return building;
        }
    }
}