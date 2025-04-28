namespace MainCore.Commands.Queries
{
    [Handler]
    public static partial class GetMissingResourceQuery
    {
        public sealed record Query(VillageId VillageId, long[] RequiredResource) : ICustomQuery;

        private static async ValueTask<long[]> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var (villageId, requiredResource) = query;
            using var context = await contextFactory.CreateDbContextAsync();

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