namespace MainCore.Commands.Misc
{
    [Handler]
    public sealed partial class OpenBrowserCommand(IChromeBrowser browser, IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(AccountId AccountId, AccessDto Access) : IAccountCommand;

        private async ValueTask HandleAsync(Command command)
        {
            var (accountId, access) = command;

            var (username, server, headless) = GetAccount(accountId);
            var chromeSetting = new ChromeSetting()
            {
                UserAgent = access.Useragent,
                ProfilePath = GetProfilePath(server, username),
                ProxyHost = access.ProxyHost,
                ProxyPort = access.ProxyPort,
                ProxyUsername = access.ProxyUsername,
                ProxyPassword = access.ProxyPassword,
                IsHeadless = headless,
            };

            await browser.Setup(chromeSetting);
            await browser.Navigate($"{server}");

            UpdateLastUsed(access);
        }

        private static string GetProfilePath(string server, string username)
        {
            var uri = new Uri(server);
            var serverFolderName = uri.Host.Replace(".", "_");
            return Path.Combine(serverFolderName, username);
        }

        private (string Username, string Server, bool headless) GetAccount(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var account = context.Accounts
               .Where(x => x.Id == accountId.Value)
               .Select(x => new
               {
                   x.Username,
                   x.Server
               })
               .First();

            var headless = context.BooleanByName(accountId, AccountSettingEnums.HeadlessChrome);
            return (account.Username, account.Server, headless);
        }

        private void UpdateLastUsed(AccessDto access)
        {
            using var context = contextFactory.CreateDbContext();
            context.Accesses
               .Where(x => x.Id == access.Id.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}