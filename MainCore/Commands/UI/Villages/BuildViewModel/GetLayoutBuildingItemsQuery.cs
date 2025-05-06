using Humanizer;
using MainCore.Constraints;
using MainCore.UI.Models.Output;
using System.Text;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class GetLayoutBuildingItemsQuery
    {
        public sealed record Query(VillageId VillageId) : IQuery;

        private static async ValueTask<List<ListBoxItem>> HandleAsync(
            Query query,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            CancellationToken cancellationToken
            )
        {
            var buildings = await getLayoutBuildingsQuery.HandleAsync(new(query.VillageId), cancellationToken);
            var items = buildings
                .Select(ToListBoxItem)
                .ToList();
            return items;
        }

        private static ListBoxItem ToListBoxItem(BuildingItem building)
        {
            const string arrow = " -> ";
            var sb = new StringBuilder();
            sb.Append(building.CurrentLevel);
            if (building.QueueLevel != 0)
            {
                var content = $"{arrow}({building.QueueLevel})";
                sb.Append(content);
            }
            if (building.JobLevel != 0)
            {
                var content = $"{arrow}[{building.JobLevel}]";
                sb.Append(content);
            }

            var item = new ListBoxItem()
            {
                Id = building.Id.Value,
                Content = $"[{building.Location}] {building.Type.Humanize()} | lvl {sb}",
                Color = building.Type.GetColor(),
            };
            return item;
        }
    }
}