using System.Collections.Concurrent;

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
            _dictionary.TryAdd(accountId, browser);
            return browser;
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