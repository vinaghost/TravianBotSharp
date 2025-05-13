using MainCore.Constraints;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class OpenBrowserCommand
    {
        public sealed record Command(AccountId AccountId, AccessDto Access) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser, AppDbContext context, ILogger logger,
            CancellationToken cancellationToken
            )
        {
            var (accountId, access) = command;

            var account = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Select(x => new
                {
                    x.Username,
                    x.Server,
                })
                .First();

            var uri = new Uri(account.Server);

            var serverFolderName = uri.Host.Replace(".", "_");
            var accountFolderName = account.Username;

            var headlessChrome = context.BooleanByName(accountId, AccountSettingEnums.HeadlessChrome);
            var profilePath = Path.Combine(serverFolderName, accountFolderName);

            var chromeSetting = new ChromeSetting()
            {
                UserAgent = access.Useragent,
                ProfilePath = profilePath,
                ProxyHost = access.ProxyHost,
                ProxyPort = access.ProxyPort,
                ProxyUsername = access.ProxyUsername,
                ProxyPassword = access.ProxyPassword,
                IsHeadless = headlessChrome,
            };

            await browser.Setup(chromeSetting, logger);
            await browser.Navigate($"{account.Server}", cancellationToken);

            context.Accesses
               .Where(x => x.Id == access.Id.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}