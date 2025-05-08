using MainCore.Constraints;

namespace MainCore.Commands.UI.EditAccountViewModel
{
    [Handler]
    public static partial class GetAcccountQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<AccountDto> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

            var account = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Include(x => x.Accesses)
                .ToDto()
                .FirstOrDefault();
            return account;
        }
    }
}