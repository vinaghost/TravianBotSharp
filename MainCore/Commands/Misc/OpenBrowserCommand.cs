using MainCore.Constraints;
using System.Text.Json;
using MainCore.DTO;

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

            var cookieData = context.Accesses
               .Where(x => x.Id == access.Id.Value)
               .Select(x => x.Cookies)
               .FirstOrDefault();

            if (!string.IsNullOrEmpty(cookieData))
            {
                var cookieDtos = JsonSerializer.Deserialize<List<CookieDto>>(cookieData);
                if (cookieDtos is not null && cookieDtos.Count > 0)
                {
                    await browser.SetCookies(cookieDtos.Select(c => c.ToCookie()));
                    await browser.Navigate($"{account.Server}/dorf1.php", cancellationToken);
                }
            }

            context.Accesses
               .Where(x => x.Id == access.Id.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}