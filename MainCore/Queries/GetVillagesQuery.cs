using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetVillagesQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<List<VillageId>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

            var items = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new VillageId(x.Id))
                .ToList();

            return items;
        }
    }
}