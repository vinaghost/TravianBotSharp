using MainCore.Commands.Base;

namespace MainCore.Commands.Queries
{
    [Handler]
    public static partial class GetVillagesQuery
    {
        public sealed record Query(AccountId AccountId) : IQuery;

        private static async ValueTask<List<VillageId>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var accountId = query.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();

            var items = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new VillageId(x.Id))
                .ToList();

            return items;
        }
    }
}