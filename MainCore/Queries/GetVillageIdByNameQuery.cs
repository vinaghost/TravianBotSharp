using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetVillageIdByNameQuery
    {
        public sealed record Query(AccountId AccountId, string Name) : IAccountQuery;

        private static async ValueTask<VillageId?> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (accountId, name) = query;

            var id = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Name == name)
                .Select(x => x.Id)
                .FirstOrDefault();
            if (id == 0) return null;
            return new VillageId(id);
        }
    }
}
