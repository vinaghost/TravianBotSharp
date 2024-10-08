using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetMissingResource>]
    public class GetMissingResource(IDbContextFactory<AppDbContext> contextFactory) : QueryBase(contextFactory)
    {
        public long[] Execute(VillageId villageId, long[] requiredResource)
        {
            using var context = _contextFactory.CreateDbContext();
            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();
            if (storage is null) return [0, 0, 0, 0];

            var resource = new long[4];
            if (storage.Wood < requiredResource[0]) resource[0] = requiredResource[0] - storage.Wood;
            if (storage.Clay < requiredResource[1]) resource[1] = requiredResource[1] - storage.Clay;
            if (storage.Iron < requiredResource[2]) resource[2] = requiredResource[2] - storage.Iron;
            if (storage.Crop < requiredResource[3]) resource[3] = requiredResource[3] - storage.Crop;
            return resource;
        }
    }
}