using System.Collections.Concurrent;
using System.Reflection;

namespace MainCore.Services
{
    [RegisterSingleton<IChromeManager, ChromeManager>]
    public sealed class ChromeManager(ILogger logger) : IChromeManager
    {
        private readonly ILogger _logger = logger.ForContext<ChromeManager>();
        private readonly ConcurrentDictionary<AccountId, IBrowser> _dictionary = new();
        private string[] _extensionsPath = default!;

        public IBrowser Get(AccountId accountId)
        {
            if (_dictionary.TryGetValue(accountId, out IBrowser? browser))
            {
                return browser;
            }
            browser = new ChromeBrowser(_extensionsPath);
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
                Directory.CreateDirectory(extenstionDir);
                _logger.Information("Create directory {ExtenstionDir} for extension files.", extenstionDir);
            }

            var asmb = Assembly.GetExecutingAssembly();
            var extensionsName = asmb.GetManifestResourceNames();
            var list = new List<string>();

            foreach (var extensionName in extensionsName)
            {
                if (!extensionName.Contains(".crx")) continue;
                var path = Path.Combine(extenstionDir, extensionName);
                list.Add(path);

                if (!File.Exists(path))
                {
                    using Stream input = asmb.GetManifestResourceStream(extensionName)!;
                    using Stream output = File.Create(path);
                    input.CopyTo(output);
                    _logger.Information("Copy default extension file {ExtensionName} to {Path}.", extensionName, path);
                }
            }

            _extensionsPath = list.ToArray();
            _logger.Information("Loaded {Count} extension files.", _extensionsPath.Length);
        }
    }
}
