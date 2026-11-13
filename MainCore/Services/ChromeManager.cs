using System.Collections.Concurrent;

using Serilog;

namespace MainCore.Services
{
    [RegisterSingleton<ChromeManager>]
    public sealed class ChromeManager(IDbContextFactory<AppDbContext> contextFactory)
    {
        private readonly ConcurrentDictionary<AccountId, IChromeBrowser> _dictionary = new();

        public IChromeBrowser Get(AccountId accountId)
        {
            if (_dictionary.TryGetValue(accountId, out IChromeBrowser? browser))
            {
                return browser;
            }

            browser = new ChromeBrowser(contextFactory);
            browser.SetAccountId(accountId);
            var logger = Log
                .ForContext("Account", GetAccountData(accountId))
                .ForContext("AccountId", accountId);
            browser.Logger = logger;

            _dictionary.TryAdd(accountId, browser);
            return browser;
        }

        private string GetAccountData(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            return context.GetAccountData(accountId);
        }

        public async Task Shutdown()
        {
            foreach (var id in _dictionary.Keys)
            {
                if (_dictionary.Remove(id, out IChromeBrowser? browser))
                {
                    await browser.Shutdown();
                }
            }
        }
    }
}