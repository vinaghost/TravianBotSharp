using MainCore.Constraints;
using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.VillageViewModel
{
    [Handler]
    public static partial class GetVillageItemsQuery
    {
        public sealed record Query(AccountId AccountId) : IQuery;

        private static async ValueTask<List<ListBoxItem>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

            var items = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .OrderBy(x => x.Name)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Content = $"{x.Name}{Environment.NewLine}({x.X}|{x.Y})",
                })
                .ToList();

            return items;
        }
    }
}