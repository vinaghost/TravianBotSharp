﻿using MainCore.Common.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace MainCore.Services
{
    public sealed class ChromeBrowser : IChromeBrowser
    {
        private ChromeDriver _driver;
        private readonly ChromeDriverService _chromeService;
        private WebDriverWait _wait;

        private readonly string[] _extensionsPath;
        private readonly HtmlDocument _htmlDoc = new();

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
            options.AddArgument("--ignore-certificate-errors");
            options.AddArguments("--no-default-browser-check", "--no-first-run", "--ash-no-nudges");
            options.AddArguments("--mute-audio", "--disable-gpu", "--disable-search-engine-choice-screen");

            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", "undefined");

            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-features=CalculateNativeWinOcclusion");
            options.AddArgument("--disable-features=UserAgentClientHint");
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

            var result = await Result.Try(() => Task.Run(refresh), Retry.Exception);
            return result;
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
            var result = await Result.Try(() => Task.Run(goToUrl), Retry.Exception);
            return result;
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

                var result = await Result.Try(() => Task.Run(specialClick), Retry.Exception);
                return result;
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

            var result = await Result.Try(() => Task.Run(input), Retry.Exception);
            return result;
        }

        private async Task<Result> ExecuteJsScript(string javascript)
        {
            var js = Driver as IJavaScriptExecutor;
            void execute()
            {
                js.ExecuteScript(javascript);
            }

            var result = await Result.Try(() => Task.Run(execute), Retry.Exception);
            return result;
        }

        public async Task<Result> ExecuteJsScript(string javascript, string url, CancellationToken cancellationToken)
        {
            Result result;
            result = await ExecuteJsScript(javascript);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await WaitPageChanged(url, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        public async Task<Result> Wait(Func<IWebDriver, bool> condition, CancellationToken cancellationToken)
        {
            void wait()
            {
                _wait.Until(driver =>
                {
                    if (cancellationToken.IsCancellationRequested) return true;
                    return condition(driver);
                });
            }

            var result = await Result.Try(() => Task.Run(wait), ex => ex is WebDriverTimeoutException ? Stop.PageNotLoad : Retry.Exception(ex));
            if (cancellationToken.IsCancellationRequested) return Cancel.Error;
            return result;
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