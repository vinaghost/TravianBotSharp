namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class StorageRepository : IStorageRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public StorageRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public int GetGranaryPercent(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var percent = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Crop * 100f / x.Granary)
                .FirstOrDefault();
            return (int)percent;
        }
    }
}