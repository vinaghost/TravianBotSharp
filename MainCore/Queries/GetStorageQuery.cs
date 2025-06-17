using MainCore.Constraints;
using MainCore.DTO;
using MainCore.Infrasturecture.Persistence;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetStorageQuery
    {
        public sealed record Query(VillageId VillageId) : IVillageQuery;

        private static async ValueTask<StorageDto> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var villageId = query.VillageId;

            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => new StorageDto
                {
                    Wood = x.Wood,
                    Clay = x.Clay,
                    Iron = x.Iron,
                    Crop = x.Crop,
                    ProductionWood = x.ProductionWood,
                    ProductionClay = x.ProductionClay,
                    ProductionIron = x.ProductionIron,
                    ProductionCrop = x.ProductionCrop,
                    Warehouse = x.Warehouse,
                    Granary = x.Granary,
                    FreeCrop = x.FreeCrop
                })
                .FirstOrDefault() ?? new();

            return storage;
        }
    }
}
