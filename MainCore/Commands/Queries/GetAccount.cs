using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetAccount>]
    public class GetAccount(IDbContextFactory<AppDbContext> contextFactory) : QueryBase(contextFactory)
    {
        public AccountDto Execute(AccountId accountId, bool includeAccess = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Accounts
                .Where(x => x.Id == accountId.Value);

            if (includeAccess)
            {
                query = query
                    .Include(x => x.Accesses);
            }
            var account = query
                .ToDto()
                .FirstOrDefault();
            return account;
        }
    }
}