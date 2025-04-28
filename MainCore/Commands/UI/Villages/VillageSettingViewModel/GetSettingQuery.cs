namespace MainCore.Commands.UI.Villages.VillageSettingViewModel
{
    [Handler]
    public static partial class GetSettingQuery
    {
        public sealed record Query(VillageId VillageId) : ICustomQuery;

        private static async ValueTask<Dictionary<VillageSettingEnums, int>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var villageId = query.VillageId;
            using var context = await contextFactory.CreateDbContextAsync();

            var settings = context.VillagesSetting
               .Where(x => x.VillageId == villageId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}