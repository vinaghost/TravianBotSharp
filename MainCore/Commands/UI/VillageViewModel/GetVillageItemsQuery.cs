using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.VillageViewModel
{
    [Handler]
    public static partial class GetVillageItemsQuery
    {
        public sealed record Query(AccountId AccountId) : ICustomQuery;

        private static async ValueTask<List<ListBoxItem>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var accountId = query.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();

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