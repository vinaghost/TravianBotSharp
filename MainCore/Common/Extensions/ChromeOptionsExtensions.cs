using OpenQA.Selenium.Chrome;
using System.IO.Compression;

namespace MainCore.Common.Extensions
{
    public static class ChromeOptionsExtensions
    {
        private const string background_js = @"
var config = {
	mode: ""fixed_servers"",
    rules: {
        singleProxy: {
            scheme: ""http"",
            host: ""{HOST}"",
            port: parseInt({PORT})
        },
        bypassList: []
	}
};

chrome.proxy.settings.set({ value: config, scope: ""regular"" }, function() { });

function callbackFn(details)
{
	return {
		authCredentials:
		{
			username: ""{USERNAME}"",
			password: ""{PASSWORD}""
		}
	};
}

chrome.webRequest.onAuthRequired.addListener(
	callbackFn,
	{ urls:[""<all_urls>""] },
    ['blocking']
);";

        private const string manifest_json = @"
{
    ""version"": ""1.0.0"",
    ""manifest_version"": 3,
    ""name"": ""Chrome Proxy Authentication"",
    ""permissions"": [
        ""proxy"",
        ""tabs"",
        ""unlimitedStorage"",
        ""storage"",
        ""webRequest"",
        ""webRequestAuthProvider""
    ],
    ""host_permissions"": [
        ""<all_urls>""
    ],
    ""background"": {
        ""service_worker"": ""background.js""
    },
    ""minimum_chrome_version"": ""108""
}";

        /// <summary>
        /// Add HTTP-proxy by <paramref name="userName"/> and <paramref name="password"/>
        /// </summary>
        /// <param name="options">Chrome options</param>
        /// <param name="host">Proxy host</param>
        /// <param name="port">Proxy port</param>
        /// <param name="userName">Proxy username</param>
        /// <param name="password">Proxy password</param>
        public static void AddHttpProxy(this ChromeOptions options, string host, int port, string userName, string password)
        {
            var background_proxy_js = ReplaceTemplates(background_js, host, port, userName, password);

            if (!Directory.Exists("Plugins"))
                Directory.CreateDirectory("Plugins");

            var guid = Guid.NewGuid().ToString();

            var manifestPath = $"Plugins/manifest_{guid}.json";
            var backgroundPath = $"Plugins/background_{guid}.js";
            var archiveFilePath = $"Plugins/proxy_auth_plugin_{guid}.zip";

            File.WriteAllText(manifestPath, manifest_json);
            File.WriteAllText(backgroundPath, background_proxy_js);

            using (var zip = ZipFile.Open(archiveFilePath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(manifestPath, "manifest.json");
                zip.CreateEntryFromFile(backgroundPath, "background.js");
            }

            File.Delete(manifestPath);
            File.Delete(backgroundPath);

            options.AddExtension(archiveFilePath);
        }

        private static string ReplaceTemplates(string str, string host, int port, string userName, string password)
        {
            return str
                .Replace("{HOST}", host)
                .Replace("{PORT}", port.ToString())
                .Replace("{USERNAME}", userName)
                .Replace("{PASSWORD}", password);
        }
    }
}