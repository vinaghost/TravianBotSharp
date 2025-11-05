using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;

namespace MainCore.Services
{
    [RegisterSingleton<IChromeManager, ChromeManager>]
    public sealed class ChromeManager(ILogger logger) : IChromeManager
    {
        private readonly ILogger _logger = logger.ForContext<ChromeManager>();
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
                _logger.Information("Create directory {ExtenstionDir} for extension files.", extenstionDir);
            }

            var asmb = Assembly.GetExecutingAssembly();
            var extensionsName = asmb.GetManifestResourceNames();
            var list = new List<string>();

            foreach (var extensionName in extensionsName)
            {
                if (!extensionName.Contains(".crx")) continue;
                var archiveFilePath = Path.Combine(extenstionDir, extensionName);

                if (!File.Exists(archiveFilePath))
                {
                    using Stream input = asmb.GetManifestResourceStream(extensionName)!;
                    using Stream output = File.Create(archiveFilePath);
                    input.CopyTo(output);
                    _logger.Information("Copy default extension file {ExtensionName} to {Path}.", extensionName, archiveFilePath);
                }

                var extensionDirPath = Path.Combine(extenstionDir, Path.GetFileNameWithoutExtension(archiveFilePath));
                UnpackCrx(archiveFilePath, extensionDirPath);

                list.Add(extensionDirPath);
            }

            _extensionsPath = list.ToArray();
            _logger.Information("Loaded {Count} extension files.", _extensionsPath.Length);
        }

        public static void UnpackCrx(string crxPath, string outputDir)
        {
            using var fs = new FileStream(crxPath, FileMode.Open, FileAccess.Read);

            // Read magic and version
            byte[] header = new byte[12];
            fs.Read(header, 0, 12);

            if (header[0] != (byte)'C' || header[1] != (byte)'r' || header[2] != (byte)'2' || header[3] != (byte)'4')
                throw new InvalidDataException("Not a valid CRX file.");

            int version = BitConverter.ToInt32(header, 4);

            long zipStartOffset;
            if (version == 2)
            {
                // CRX v2 header: magic(4) + version(4) + pubkey_len(4) + sig_len(4)
                byte[] v2Header = new byte[8];
                fs.Read(v2Header, 0, 8);
                int pubkeyLen = BitConverter.ToInt32(v2Header, 0);
                int sigLen = BitConverter.ToInt32(v2Header, 4);
                zipStartOffset = 16 + pubkeyLen + sigLen;
            }
            else if (version == 3)
            {
                // CRX v3 header: magic(4) + version(4) + header_len(4)
                int headerLen = BitConverter.ToInt32(header, 8);
                zipStartOffset = 12 + headerLen;
            }
            else
            {
                throw new InvalidDataException($"Unsupported CRX version: {version}");
            }

            // Skip to ZIP data
            fs.Seek(zipStartOffset, SeekOrigin.Begin);

            using var zipStream = new MemoryStream();
            fs.CopyTo(zipStream);
            zipStream.Position = 0;

            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
            zip.ExtractToDirectory(outputDir, true);
        }
    }
}