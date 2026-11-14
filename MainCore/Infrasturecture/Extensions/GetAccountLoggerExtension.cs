namespace MainCore.Infrasturecture.Extensions
{
    public static class GetAccountLoggerExtension
    {
        extension(AppDbContext context)
        {
            public string GetAccountData(AccountId accountId)
            {
                var account = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Select(x => new
                {
                    x.Username,
                    x.Server,
                })
                .FirstOrDefault() ?? throw new InvalidOperationException($"Account with ID {accountId.Value} not found.");
                var uri = new Uri(account.Server);
                return $"{account.Username}_{uri.Host}";
            }
        }
    }
}