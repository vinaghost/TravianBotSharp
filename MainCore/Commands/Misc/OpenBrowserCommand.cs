using MainCore.Common.Models;

namespace MainCore.Commands.Misc
{
    [RegisterSingleton<OpenBrowserCommand>]
    public class OpenBrowserCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public OpenBrowserCommand(IChromeManager chromeManager, IDbContextFactory<AppDbContext> contextFactory)
        {
            _chromeManager = chromeManager;
            _contextFactory = contextFactory;
        }

        public async Task<Result> Execute(AccountId accountId, AccessDto access, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(accountId);

            var getAccount = Locator.Current.GetService<GetAccount>();
            var account = getAccount.Execute(accountId);
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
            var result = await chromeBrowser.Setup(chromeSetting);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.Navigate($"{account.Server}/dorf1.php", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            UpdateAccessLastUsed(access.Id);
            return Result.Ok();
        }

        private void UpdateAccessLastUsed(AccessId accessId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Accesses
               .Where(x => x.Id == accessId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}