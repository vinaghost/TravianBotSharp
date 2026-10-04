using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MainCore.Services
{
    public sealed class ChromeBrowser : IChromeBrowser
    {
        private IPlaywright? _playwright;
        private IBrowserContext? _browser;
        private IPage? _mainPage;

        public IPage CurrentPage => _mainPage ?? throw new InvalidOperationException("Main page is not initialized.");

        public string CurrentUrl => _mainPage?.Url ?? "";

        public ILogger Logger { get; set; } = null!;
        public IDelayService DelayService { get; set; } = null!;

        public bool IsInitialized => _playwright is not null && _browser is not null && _mainPage is not null;
        public bool IsHeadless { get; private set; }

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
                    "--disable-search-engine-choice-screen",
                    "--webrtc-ip-handling-policy=disable_non_proxied_udp",
                    "--force-webrtc-ip-handling-policy",
                ],
            });
            IsHeadless = setting.IsHeadless;
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

        public async Task Shutdown()
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during shutdown: {Message}", ex.Message);
            }
        }

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
            try
            {
                await _mainPage.GotoAsync(url);
                return Result.Ok();
            }
            catch (TimeoutException ex)
            {
                return Retry.Error.WithError($"Navigating to URL [{url}] timed out. Details: {ex.Message}");
            }
        }

        public async Task<Result> Click(ILocator locator, [CallerArgumentExpression(nameof(locator))] string? expression = null)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            try
            {
                await locator.ClickAsync();
                await DelayService.DelayClick();
                return Result.Ok();
            }
            catch (TimeoutException ex)
            {
                return Retry.Error.WithError($"Clicking locator [{expression}] timed out. Details: {ex.Message}");
            }
        }

        public async Task<Result> Input(ILocator locator, string content, [CallerArgumentExpression(nameof(locator))] string? expression = null)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            try
            {
                await locator.FillAsync(content);
                await DelayService.DelayClick();
                return Result.Ok();
            }
            catch (TimeoutException ex)
            {
                return Retry.Error.WithError($"Inputting into locator [{expression}] timed out. Details: {ex.Message}");
            }
        }

        public async Task<Result> ExecuteJsScript(string javascript)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            await _mainPage.EvaluateAsync(javascript);
            await DelayService.DelayClick();

            return Result.Ok();
        }

        public async Task<Result> Wait(ILocator locator, [CallerArgumentExpression(nameof(locator))] string? expression = null)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            try
            {
                await locator.WaitForAsync();
                return Result.Ok();
            }
            catch (TimeoutException ex)
            {
                return Retry.Error.WithError($"Waiting for locator [{expression}] timed out. Details: {ex.Message}");
            }
        }

        public async Task<Result> Wait(ILocator locator, string condition, [CallerArgumentExpression(nameof(locator))] string? expression = null)
        {
            if (_mainPage is null) return Stop.DriverNotReady;
            try
            {
                await locator.WaitForFunctionAsync(condition);
                return Result.Ok();
            }
            catch (TimeoutException ex)
            {
                return Retry.Error.WithError($"Waiting for locator [{expression}] timed out. Details: {ex.Message}");
            }
        }

        public async Task<Result> WaitPageChanged(string url)
        {
            if (_mainPage is null) return Stop.DriverNotReady;

            try
            {
                const int timeout = 180_000; // 3 minutes in milliseconds
                await _mainPage.WaitForURLAsync(new Regex(url, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)), new() { Timeout = timeout });
                await _mainPage.WaitForLoadStateAsync();
                await LoginParser.GetServerTime(_mainPage).WaitForAsync(new() { Timeout = timeout });
                return Result.Ok();
            }
            catch (TimeoutException ex)
            {
                string actualUrl = _mainPage.Url;
                return Retry.Error.WithError($"Navigation or page load failed. Expected URL: [{url}]. Current URL: [{actualUrl}]. Details: {ex.Message}");
            }
        }
    }
}