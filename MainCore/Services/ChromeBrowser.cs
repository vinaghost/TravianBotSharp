using MainCore.Common.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace MainCore.Services
{
    [DoNotAutoRegister]
    public sealed class ChromeBrowser : IChromeBrowser
    {
        private ChromeDriver _driver;
        private readonly ChromeDriverService _chromeService;
        private WebDriverWait _wait;

        private readonly string[] _extensionsPath;
        private readonly HtmlDocument _htmlDoc = new();

        public string EndpointAddress
        {
            get
            {
                if (_driver is null) return "";
                return _driver.GetDevToolsSession().EndpointAddress;
            }
        }

        public ChromeBrowser(string[] extensionsPath)
        {
            _extensionsPath = extensionsPath;

            _chromeService = ChromeDriverService.CreateDefaultService();
            _chromeService.HideCommandPromptWindow = true;
        }

        public async Task<Result> Setup(ChromeSetting setting)
        {
            var options = new ChromeOptions();

            options.AddExtensions(_extensionsPath);

            if (!string.IsNullOrEmpty(setting.ProxyHost))
            {
                if (!string.IsNullOrEmpty(setting.ProxyUsername))
                {
                    options.AddHttpProxy(setting.ProxyHost, setting.ProxyPort, setting.ProxyUsername, setting.ProxyPassword);
                }
                else
                {
                    options.AddArgument($"--proxy-server={setting.ProxyHost}:{setting.ProxyPort}");
                }
            }

            options.AddArgument($"--user-agent={setting.UserAgent}");

            // So websites (Travian) can't detect the bot
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-features=UserAgentClientHint");
            options.AddArgument("--disable-logging");
            options.AddArgument("--ignore-certificate-errors");

            options.AddArguments("--mute-audio", "--disable-gpu");

            options.AddArguments("--no-default-browser-check", "--no-first-run");
            options.AddArguments("--no-sandbox", "--test-type");

            if (setting.IsHeadless)
            {
                options.AddArgument("--headless=new");
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
            _driver.GetDevToolsSession();
            _wait = new WebDriverWait(_driver, TimeSpan.FromMinutes(3)); // watch ads

            return Result.Ok();
        }

        public ChromeDriver Driver => _driver;

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

        public string CurrentUrl => _driver.Url;

        private async Task<Result> Refresh()
        {
            void refresh()
            {
                _driver.Navigate().Refresh();
            }

            try
            {
                await Task.Run(refresh);
                return Result.Ok();
            }
            catch (Exception exception)
            {
                return Stop.Exception(exception);
            }
        }

        public async Task<Result> Refresh(CancellationToken cancellationToken)
        {
            Result result;
            result = await Refresh();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await Task.Delay(600, CancellationToken.None);

            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> Navigate(string url)
        {
            void goToUrl()
            {
                _driver.Navigate().GoToUrl(url);
            }

            try
            {
                await Task.Run(goToUrl);
                return Result.Ok();
            }
            catch (Exception exception)
            {
                return Stop.Exception(exception);
            }
        }

        public async Task<Result> Navigate(string url, CancellationToken cancellationToken)
        {
            Result result;
            result = await Navigate(url);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await Task.Delay(600, CancellationToken.None);

            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> Click(By by)
        {
            var elements = _driver.FindElements(by);
            if (elements.Count == 0) return Retry.ElementNotFound();
            var element = elements[0];
            if (!element.Displayed || !element.Enabled) return Retry.ElementNotClickable();
            try
            {
                var normalClick = element.Click;
                await Task.Run(normalClick);
            }
            catch
            {
                var specialClick = new Actions(_driver).Click(element).Perform;
                await Task.Run(specialClick);
            }

            return Result.Ok();
        }

        public async Task<Result> Click(By by, CancellationToken cancellationToken)
        {
            Result result;
            result = await Click(by);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await Task.Delay(600, CancellationToken.None);

            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        public async Task<Result> Click(By by, string url, CancellationToken cancellationToken)
        {
            Result result;
            result = await Click(by);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await WaitPageChanged(url, cancellationToken);
            if (result.IsFailed)
            {
                result = await Click(by);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await WaitPageChanged(url, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        public async Task<Result> Click(By by, string url, Func<IWebDriver, bool> condition, CancellationToken cancellationToken)
        {
            Result result;
            result = await Click(by);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await WaitPageChanged(url, cancellationToken);
            if (result.IsFailed)
            {
                result = await Click(by);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await WaitPageChanged(url, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await Wait(condition, cancellationToken);
            if (result.IsFailed)
            {
                result = await Click(by);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await WaitPageChanged(url, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await WaitPageLoaded(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await Wait(condition, cancellationToken);
                return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        public async Task<Result> Click(By by, Func<IWebDriver, bool> condition, CancellationToken cancellationToken)
        {
            Result result;
            result = await Click(by);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await Wait(condition, cancellationToken);
            if (result.IsFailed)
            {
                result = await Click(by);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await Wait(condition, cancellationToken);
                return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        public async Task<Result> InputTextbox(By by, string content)
        {
            var elements = _driver.FindElements(by);
            if (elements.Count == 0) return Retry.ElementNotFound();

            var element = elements[0];
            if (!element.Displayed || !element.Enabled) return Retry.ElementNotClickable();

            void input()
            {
                element.SendKeys(Keys.Home);
                element.SendKeys(Keys.Shift + Keys.End);
                element.SendKeys(content);
            }
            await Task.Run(input);

            return Result.Ok();
        }

        private async Task ExecuteJsScript(string javascript)
        {
            var js = Driver as IJavaScriptExecutor;
            void execute()
            {
                js.ExecuteScript(javascript);
            }

            await Task.Run(execute);
        }

        public async Task<Result> ExecuteJsScript(string javascript, string url, CancellationToken cancellationToken)
        {
            await ExecuteJsScript(javascript);

            Result result;
            result = await WaitPageChanged(url, cancellationToken);
            if (result.IsFailed)
            {
                await ExecuteJsScript(javascript);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await WaitPageChanged(url, cancellationToken);
                return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        public async Task<Result> Wait(Func<IWebDriver, bool> condition, CancellationToken cancellationToken)
        {
            Result wait()
            {
                _wait.Until(driver =>
                {
                    if (cancellationToken.IsCancellationRequested) return true;
                    return condition(driver);
                });
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
                return Result.Ok();
            }

            try
            {
                return await Task.Run(wait);
            }
            catch (WebDriverTimeoutException)
            {
                return Stop.PageNotLoad;
            }
        }

        public async Task<Result> WaitPageLoaded(CancellationToken cancellationToken)
        {
            static bool pageLoaded(IWebDriver driver) => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");

            var result = await Wait(pageLoaded, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return result;
        }

        public async Task<Result> WaitPageChanged(string part, CancellationToken cancellationToken)
        {
            bool pageChanged(IWebDriver driver) => driver.Url.Contains(part);
            var result = await Wait(pageChanged, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return result;
        }

        public async Task Close()
        {
            if (_driver is null) return;
            await Task.Run(_driver.Quit);
            _driver = null;
        }
    }
}