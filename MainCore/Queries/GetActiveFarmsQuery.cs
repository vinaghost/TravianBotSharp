using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetActiveFarmsQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<List<FarmId>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken token
        )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

            var farms = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Select(x => new FarmId(x.Id))
                .ToList();
            return farms;
        }
    }
}