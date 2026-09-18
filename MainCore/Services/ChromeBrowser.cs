using Microsoft.Playwright;

namespace MainCore.Services
{
    public sealed class ChromeBrowser : IChromeBrowser
    {
        private IPlaywright? _playwright;
        private IBrowserContext? _browser;
        private IPage? _mainPage;
        private readonly HtmlDocument _htmlDoc = new();

        public async Task Setup(ChromeSetting setting)
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchPersistentContextAsync(GetPathUserData(setting), new()
            {
                Channel = "chrome",
                Headless = setting.IsHeadless,
                Proxy = string.IsNullOrEmpty(setting.ProxyHost) ? null : new()
                {
                    Server = $"{setting.ProxyHost}:{setting.ProxyPort}",
                    Username = setting.ProxyUsername,
                    Password = setting.ProxyPassword
                },
                ViewportSize = ViewportSize.NoViewport,
                Args =
                [
                    "--ignore-certificate-errors",
                    "--no-default-browser-check",
                    "--no-first-run",
                    "--ash-no-nudges",
                    "--mute-audio",
                    "--disable-gpu",
                    "--disable-search-engine-choice-screen"
                ],
            });

            _mainPage = await _browser.NewPageAsync();

            var firstPage = _browser.Pages.Count > 1 ? _browser.Pages[0] : null;
            if (firstPage != null && firstPage.Url == "about:blank")
            {
                await firstPage.CloseAsync();
            }
        }

        private static string GetPathUserData(ChromeSetting setting)
        {
            var pathUserData = Path.Combine(AppContext.BaseDirectory, "Data", "Cache", setting.ProfilePath);
            if (!Directory.Exists(pathUserData)) Directory.CreateDirectory(pathUserData);
            pathUserData = Path.Combine(pathUserData, string.IsNullOrEmpty(setting.ProxyHost) ? "default" : setting.ProxyHost);
            return pathUserData;
        }

        public async Task<HtmlDocument> GetHtml()
        {
            if (_mainPage is not null) _htmlDoc.LoadHtml(await _mainPage.ContentAsync());
            return _htmlDoc;
        }

        public async Task Shutdown()
        {
            if (_mainPage is not null)
            {
                await _mainPage.CloseAsync();
            }
            if (_browser is not null)
            {
                await _browser.CloseAsync();
            }
            _playwright?.Dispose();
        }

        public string CurrentUrl => _mainPage?.Url ?? "";

        public ILogger Logger { get; set; } = null!;

        public async Task<string> Screenshot()
        {
            if (_mainPage is null)
            {
                Logger.Error("Screenshot failed: main page is null");
                return "";
            }

            var screenshot = await _mainPage.ScreenshotAsync();
            if (screenshot is null)
            {
                Logger.Error("Screenshot failed: screenshot is null");
                return "";
            }

            var fileName = Path.Combine(AppContext.BaseDirectory, "Screenshots", $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
            await File.WriteAllBytesAsync(fileName, screenshot ?? [], CancellationToken.None);
            return fileName;
        }

        public async Task<Result> Refresh()
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            await _mainPage.ReloadAsync();
            return Result.Ok();
        }

        public async Task<Result> Navigate(string url)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            await _mainPage.GotoAsync(url);
            return Result.Ok();
        }

        public async Task<Result> Click(ILocator locator)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            await locator.ClickAsync();
            return Result.Ok();
        }

        public async Task<Result> Input(ILocator locator, string content)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            await locator.FillAsync(content);
            return Result.Ok();
        }

        public async Task<Result> ExecuteJsScript(string javascript)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            await _mainPage.EvaluateAsync(javascript);
            return Result.Ok();
        }

        public async Task<Result> WaitPageChanged(string url)
        {
            if (_mainPage is null) return Stop.DriverNotReady;

            try
            {
                await _mainPage.WaitForURLAsync(url);
                await _mainPage.Locator("#logo").WaitForAsync();
            }
            catch (TimeoutException ex)
            {
                string actualUrl = _mainPage.Url;
                return Retry.Error.WithError($"Navigation or page load failed. Expected URL: [{url}]. Current URL: [{actualUrl}]. Details: {ex.Message}");
            }
            return Result.Ok();
        }
    }
}