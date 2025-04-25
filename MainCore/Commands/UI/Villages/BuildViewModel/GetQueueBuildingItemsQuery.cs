using Humanizer;
using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class GetQueueBuildingItemsQuery
    {
        public sealed record Query(VillageId VillageId) : ICustomQuery;

        private static async ValueTask<List<ListBoxItem>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var villageId = query.VillageId;
            using var context = await contextFactory.CreateDbContextAsync();

            var items = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .AsEnumerable()
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Content = $"{x.Type.Humanize()} to level {x.Level} complete at {x.CompleteTime}",
                })
                .ToList();

            var tribe = (TribeEnums)context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Setting == VillageSettingEnums.Tribe)
                .Select(x => x.Value)
                .FirstOrDefault();

            var count = 2;
            if (tribe == TribeEnums.Romans) count = 3;
            items.AddRange(Enumerable.Range(0, count - items.Count).Select(x => new ListBoxItem()));
            return items;
        }
    }
}