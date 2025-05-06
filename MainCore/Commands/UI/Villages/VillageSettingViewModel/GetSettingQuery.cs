using MainCore.Constraints;

namespace MainCore.Commands.UI.Villages.VillageSettingViewModel
{
    [Handler]
    public static partial class GetSettingQuery
    {
        public sealed record Query(VillageId VillageId) : IQuery;

        private static async ValueTask<Dictionary<VillageSettingEnums, int>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var villageId = query.VillageId;

            var settings = context.VillagesSetting
               .Where(x => x.VillageId == villageId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}