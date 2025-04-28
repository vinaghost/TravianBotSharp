using MainCore.Common.Models;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class OpenBrowserCommand
    {
        public sealed record Command(AccountId accountId, AccessDto access) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeManager chromeManager, IDbContextFactory<AppDbContext> contextFactory, ILogService logService,
            CancellationToken cancellationToken
            )
        {
            var (accountId, access) = command;
            using var context = await contextFactory.CreateDbContextAsync();
            var chromeBrowser = chromeManager.Get(accountId);

            var account = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ToDto()
                .First();

            var uri = new Uri(account.Server);

            var serverFolderName = uri.Host.Replace(".", "_");
            var accountFolderName = account.Username;

            var getSetting = Locator.Current.GetService<IGetSetting>();
            var headlessChrome = getSetting.BooleanByName(accountId, AccountSettingEnums.HeadlessChrome);
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

            await chromeBrowser.Setup(chromeSetting, logService.GetLogger(accountId));
            await chromeBrowser.Navigate($"{account.Server}/dorf1.php", cancellationToken);

            context.Accesses
               .Where(x => x.Id == access.Id.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}