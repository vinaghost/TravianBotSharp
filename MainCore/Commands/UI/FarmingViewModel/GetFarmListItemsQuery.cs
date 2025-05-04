using MainCore.Commands.Base;
using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.FarmingViewModel
{
    [Handler]
    public static partial class GetFarmListItemsQuery
    {
        public sealed record Query(AccountId AccountId) : IQuery;

        private static async ValueTask<List<ListBoxItem>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
        CancellationToken cancellationToken
            )
        {
            var accountId = query.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();

            var items = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Color = x.IsActive ? SplatColor.Green : SplatColor.Red,
                    Content = x.Name,
                })
                .ToList();

            return items;
        }
    }
}