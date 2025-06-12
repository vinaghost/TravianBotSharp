using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.IO.Compression;

namespace MainCore.Services
{
    public sealed class ChromeBrowser : IChromeBrowser
    {
        private ChromeDriver? _driver;
        private readonly ChromeDriverService _chromeService;
        private WebDriverWait _wait = null!;

        private readonly string[] _extensionsPath;
        private readonly HtmlDocument _htmlDoc = new();

        public ChromeBrowser(string[] extensionsPath)
        {
            _extensionsPath = extensionsPath;

            _chromeService = ChromeDriverService.CreateDefaultService();
            _chromeService.HideCommandPromptWindow = true;
        }

        public async Task Setup(ChromeSetting setting)
        {
            var options = new ChromeOptions();

            options.AddExtensions(_extensionsPath);

            if (!string.IsNullOrEmpty(setting.ProxyHost))
            {
                if (!string.IsNullOrEmpty(setting.ProxyUsername) && !string.IsNullOrEmpty(setting.ProxyPassword))
                {
                    options.AddHttpProxy(setting.ProxyHost, setting.ProxyPort, setting.ProxyUsername, setting.ProxyPassword);
                }
                else
                {
                    options.AddArgument($"--proxy-server={setting.ProxyHost}:{setting.ProxyPort}");
                }
            }

            options.AddArgument($"--user-agent={setting.UserAgent}");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArguments("--no-default-browser-check", "--no-first-run", "--ash-no-nudges");
            options.AddArguments("--mute-audio", "--disable-gpu", "--disable-search-engine-choice-screen");

            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", "undefined");

            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-features=CalculateNativeWinOcclusion");
            options.AddArgument("--disable-features=UserAgentClientHint");
            options.AddArgument("--disable-features=DisableLoadExtensionCommandLineSwitch");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            if (setting.IsHeadless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-dev-shm-usage");
            }
            else
            {
                options.AddArgument("--start-maximized");
            }
            var pathUserData = Path.Combine(AppContext.BaseDirectory, "Data", "Cache", setting.ProfilePath);
            if (!Directory.Exists(pathUserData)) Directory.CreateDirectory(pathUserData);

            pathUserData = Path.Combine(pathUserData, string.IsNullOrEmpty(setting.ProxyHost) ? "default" : setting.ProxyHost);

            options.AddArguments($"user-data-dir={pathUserData}");

            _driver = await Task.Run(() => new ChromeDriver(_chromeService, options, TimeSpan.FromMinutes(3)));

            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(3);
            _wait = new WebDriverWait(_driver, TimeSpan.FromMinutes(3)); // watch ads
        }

        public ChromeDriver Driver => _driver!;

        public HtmlDocument Html
        {
            get
            {
                if (_driver is not null) _htmlDoc.LoadHtml(_driver.PageSource);
                return _htmlDoc;
            }
        }

        public async Task Shutdown()
        {
            if (_driver is null) return;
            await Close();
            _chromeService.Dispose();
        }

        public string CurrentUrl => Driver.Url;

        public ILogger Logger { get; set; } = null!;

        public async Task<string> Screenshot()
        {
            var screenshot = Driver.GetScreenshot();
            var fileName = Path.Combine(AppContext.BaseDirectory, "Screenshots", $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
            await File.WriteAllBytesAsync(fileName, screenshot.AsByteArray, CancellationToken.None);
            return fileName;
        }

        public async Task<Result> Refresh(CancellationToken cancellationToken)
        {
            await Driver.Navigate().RefreshAsync();
            var result = await WaitPageLoaded(cancellationToken);
            return result;
        }

        private static bool PageLoaded(IWebDriver driver) => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState")?.Equals("complete") ?? false;

        private static bool PageChanged(IWebDriver driver, string url_nested) => driver.Url.Contains(url_nested) && PageLoaded(driver);

        public async Task<Result> Navigate(string url, CancellationToken cancellationToken)
        {
            await Driver.Navigate().GoToUrlAsync(url);
            var result = await Wait(driver => PageChanged(driver, url), cancellationToken);
            return result;
        }

        public async Task<Result> Click(By by)
        {
            var elements = Driver.FindElements(by);
            if (elements.Count == 0) return Retry.ElementNotFound(by);
            var element = elements[0];
            if (!element.Displayed || !element.Enabled) return Retry.ElementNotClickable(by);
            await Task.Run(new Actions(Driver).Click(element).Perform);
            return Result.Ok();
        }

        public async Task<Result> Input(By by, string content)
        {
            var elements = Driver.FindElements(by);
            if (elements.Count == 0) return Retry.ElementNotFound(by);
            var element = elements[0];
            if (!element.Displayed || !element.Enabled) return Retry.ElementNotClickable(by);

            void input()
            {
                element.SendKeys(Keys.Home);
                element.SendKeys(Keys.Shift + Keys.End);
                element.SendKeys(content);
            }

            await Task.Run(input);
            return Result.Ok();
        }

        public async Task<Result> ExecuteJsScript(string javascript)
        {
            await Task.CompletedTask;
            var js = Driver as IJavaScriptExecutor;
            js.ExecuteScript(javascript);
            return Result.Ok();
        }

        public async Task<Result> Wait(Predicate<IWebDriver> condition, CancellationToken cancellationToken)
        {
            void wait()
            {
                _wait.Until(driver => condition(driver), cancellationToken);
            }

            try
            {
                await Task.Run(wait, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return Cancel.Error;
            }
            return Result.Ok();
        }

        public Task<Result> WaitPageLoaded(CancellationToken cancellationToken)
        {
            return Wait(PageLoaded, cancellationToken);
        }

        public Task<Result> WaitPageChanged(string part, CancellationToken cancellationToken)
        {
            return Wait(driver => PageChanged(driver, part), cancellationToken);
        }

        public Task<Result> WaitPageChanged(string part, Predicate<IWebDriver> customCondition, CancellationToken cancellationToken)
        {
            return Wait(driver => PageChanged(driver, part) && customCondition(driver), cancellationToken);
        }

        public Task<Result> WaitElement(By by, CancellationToken cancellationToken)
        {
            return Wait(driver =>
            {
                var elements = driver.FindElements(by);
                if (elements.Count == 0) return false;
                var element = elements[0];
                return element.Displayed && element.Enabled;
            }, cancellationToken);
        }

        public async Task Close()
        {
            await Task.Run(() => _driver?.Quit());
        }
    }
}

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