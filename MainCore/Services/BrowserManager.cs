using System.Collections.Concurrent;

namespace MainCore.Services
{
    [RegisterSingleton<IBrowserManager, BrowserManager>]
    public sealed class BrowserManager(ILogger logger) : IBrowserManager
    {
        private readonly ILogger _logger = logger.ForContext<BrowserManager>();
        private readonly ConcurrentDictionary<AccountId, IBrowser> _dictionary = new();
        private string[] _extensionsPath = default!;

        public IBrowser Get(AccountId accountId)
        {
            if (_dictionary.TryGetValue(accountId, out IBrowser? browser))
            {
                return browser;
            }
            
            // Firefox browser'ı varsayılan olarak kullan
            browser = new FirefoxBrowser(_extensionsPath);
            _dictionary.TryAdd(accountId, browser);
            return browser;
        }

        public async Task Shutdown()
        {
            foreach (var id in _dictionary.Keys)
            {
                if (_dictionary.Remove(id, out IBrowser? browser))
                {
                    await browser.Shutdown();
                }
            }
        }

        public void LoadExtension()
        {
            var extenstionDir = Path.Combine(AppContext.BaseDirectory, "ExtensionFile");
            if (!Directory.Exists(extenstionDir))
            {
                _logger.Warning("Extension directory not found: {ExtensionDir}", extenstionDir);
                _extensionsPath = Array.Empty<string>();
                return;
            }

            var extensions = Directory.GetFiles(extenstionDir, "*.crx", SearchOption.AllDirectories);
            _extensionsPath = extensions;
            _logger.Information("Loaded {ExtensionCount} extensions", extensions.Length);
        }
    }
}
