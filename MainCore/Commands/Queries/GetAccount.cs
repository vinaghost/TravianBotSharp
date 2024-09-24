namespace MainCore.Commands.Queries
{
    public class GetAccount(IDbContextFactory<AppDbContext> contextFactory = null)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();

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