using MainCore.Queries.Base;

namespace MainCore.Commands.UI.EditAccountViewModel
{
    [Handler]
    public static partial class GetAcccountQuery
    {
        public sealed record Query(AccountId AccountId) : IQuery;

        private static async ValueTask<AccountDto> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var accountId = query.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();

            var account = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Include(x => x.Accesses)
                .ToDto()
                .FirstOrDefault();
            return account;
        }
    }
}