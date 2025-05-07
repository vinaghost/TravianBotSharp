using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetHeroItemsQuery
    {
        public sealed record Query(AccountId AccountId, List<HeroItemEnums> ItemTypes) : IAccountQuery;

        private static async ValueTask<List<HeroItem>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken token
        )
        {
            await Task.CompletedTask;
            var (accountId, itemTypes) = query;

            var resourceItems = context.HeroItems
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => itemTypes.Contains(x.Type))
                .OrderBy(x => x.Type)
                .ToList();
            return resourceItems;
        }
    }
}