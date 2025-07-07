using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.FarmingViewModel
{
    [Handler]
    public static partial class GetFarmListItemsQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<List<ListBoxItem>> HandleAsync(
            Query query,
            AppDbContext context,
        CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

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