using Serilog;
using System.Collections.Concurrent;
using System.Reflection;

namespace MainCore.Services
{
    [RegisterSingleton<IChromeManager, ChromeManager>]
    public sealed class ChromeManager : IChromeManager
    {
        private readonly ConcurrentDictionary<AccountId, ChromeBrowser> _dictionary = new();
        private string[] _extensionsPath = default!;

        public IChromeBrowser Get(AccountId accountId)
        {
            if (_dictionary.TryGetValue(accountId, out ChromeBrowser? browser))
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
                if (_dictionary.Remove(id, out ChromeBrowser? browser))
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
                Log.Information("Create directory {extenstionDir} for extension files.", extenstionDir);
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
                    Log.Information("Copy default extension file {extensionName} to {path}.", extensionName, path);
                }
            }

            _extensionsPath = list.ToArray();
            Log.Information("Loaded {count} extension files.", _extensionsPath.Length);
        }
    }
}