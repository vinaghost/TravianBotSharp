using System.Collections.Concurrent;

namespace MainCore.Services
{
    [RegisterSingleton<ChromeManager>]
    public sealed class ChromeManager()
    {
        private readonly ConcurrentDictionary<AccountId, ChromeBrowser> _dictionary = new();

        public IChromeBrowser Get(AccountId accountId)
        {
            if (_dictionary.TryGetValue(accountId, out ChromeBrowser? browser))
            {
                return browser;
            }
            browser = new ChromeBrowser();
            _dictionary.TryAdd(accountId, browser);
            return browser;
        }

        public async Task Shutdown()
        {
            foreach (var id in _dictionary.Keys)
            {
                if (_dictionary.Remove(id, out ChromeBrowser? browser))
                {
                    await browser.Shutdown();
                }
            }
        }
    }
}