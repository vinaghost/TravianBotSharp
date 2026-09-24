using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;

namespace MainCore.Services
{
    [RegisterSingleton<IChromeManager, ChromeManager>]
    public sealed class ChromeManager() : IChromeManager
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