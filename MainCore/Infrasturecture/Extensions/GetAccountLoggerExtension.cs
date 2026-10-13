using Serilog;

namespace MainCore.Infrasturecture.Extensions
{
    public static class GetAccountLoggerExtension
    {
        extension(AppDbContext context)
        {
            public ILogger GetAccountLogger(AccountId accountId)
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
                var key = $"{account.Username}_{uri.Host}";

                var logger = Log
                    .ForContext("Account", key)
                    .ForContext("AccountId", accountId);

                return logger;
            }
        }
    }
}