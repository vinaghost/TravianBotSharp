using MainCore.Constraints;
using MainCore.DTO;
using OpenQA.Selenium;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class OpenBrowserCommand
    {
        public sealed record Command(AccountId AccountId, AccessDto Access) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser, AppDbContext context,
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

            await browser.Setup(chromeSetting);
            await browser.Navigate($"{account.Server}", cancellationToken);

            // cookies are not loaded during browser setup for now

            try
            {
                await browser.Navigate($"{account.Server}/dorf1.php", cancellationToken);
            }
            catch (WebDriverTimeoutException)
            {
                // navigation may redirect to the login page which does not contain
                // dorf1.php in the URL. Ignore this timeout so login can proceed.
            }

            context.Accesses
               .Where(x => x.Id == access.Id.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}