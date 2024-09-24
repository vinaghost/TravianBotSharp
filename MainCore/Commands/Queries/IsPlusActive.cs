namespace MainCore.Commands.Queries
{
    public class IsPlusActive(IDbContextFactory<AppDbContext> contextFactory = null)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();

        public bool Execute(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var accountInfo = context.AccountsInfo
                    .FirstOrDefault(x => x.AccountId == accountId.Value);

            if (accountInfo is null) return false;
            return accountInfo.HasPlusAccount;
        }
    }
}