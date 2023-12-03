using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Errors;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
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

        public ChromeBrowser(string[] extensionsPath)
        {
            _extensionsPath = extensionsPath;

            _chromeService = ChromeDriverService.CreateDefaultService();
            _chromeService.HideCommandPromptWindow = true;
        }

        public async Task<Result> Setup(AccountDto account, AccessDto access)
        {
            var options = new ChromeOptions();

            options.AddExtensions(_extensionsPath);

            if (!string.IsNullOrEmpty(access.ProxyHost))
            {
                if (!string.IsNullOrEmpty(access.ProxyUsername))
                {
                    options.AddHttpProxy(access.ProxyHost, access.ProxyPort, access.ProxyUsername, access.ProxyPassword);
                }
                else
                {
                    options.AddArgument($"--proxy-server={access.ProxyHost}:{access.ProxyPort}");
                }
            }

            options.AddArgument($"--user-agent={access.Useragent}");

            // So websites (Travian) can't detect the bot
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-features=UserAgentClientHint");
            options.AddArgument("--disable-logging");

            options.AddArgument("--mute-audio");

            options.AddArguments("--no-default-browser-check", "--no-first-run");
            options.AddArguments("--no-sandbox", "--test-type");

            options.AddArguments("--start-maximized");

            //if (setting.IsDontLoadImage) options.AddArguments("--blink-settings=imagesEnabled=false"); //--disable-images
            var pathUserData = Path.Combine(AppContext.BaseDirectory, "Data", "Cache", account.Server.Replace("https://", "").Replace("http://", "").Replace(".", "_"), account.Username);
            if (!Directory.Exists(pathUserData)) Directory.CreateDirectory(pathUserData);

            pathUserData = Path.Combine(pathUserData, string.IsNullOrEmpty(access.ProxyHost) ? "default" : access.ProxyHost);

            options.AddArguments($"user-data-dir={pathUserData}");

            _driver = await Task.Run(() => new ChromeDriver(_chromeService, options));
            //if (setting.IsMinimized) _driver.Manage().Window.Minimize();

            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(1);
            _wait = new WebDriverWait(_driver, TimeSpan.FromMinutes(3));

            var result = await Navigate($"{account.Server}dorf1.php");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        public ChromeDriver Driver => _driver;

        public HtmlDocument Html
        {
            get
            {
                UpdateHtml();
                return _htmlDoc;
            }
        }

        public async Task Shutdown()
        {
            if (_driver is null) return;

            try
            {
                await Task.Run(_driver.Quit);
            }
            catch { }
            _driver = null;
            _chromeService.Dispose();
        }

        public bool IsOpen()
        {
            try
            {
                _ = _driver.Title;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string CurrentUrl => _driver.Url;

        public async Task<Result> Navigate(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return await Navigate(CurrentUrl);
            }

            Result goToUrl()
            {
                try
                {
                    _driver.Navigate().GoToUrl(url);
                    return Result.Ok();
                }
                catch (Exception exception)
                {
                    return Result.Fail(new Stop(exception.Message));
                }
            }
            var result = await Task.Run(goToUrl);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await WaitPageLoaded();
            return result;
        }

        private void UpdateHtml(string source = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                try
                {
                    _htmlDoc.LoadHtml(_driver.PageSource);
                }
                catch { }
            }
            else
            {
                _htmlDoc.LoadHtml(source);
            }
        }

        public async Task<Result> Click(By by)
        {
            var elements = _driver.FindElements(by);
            if (elements.Count == 0) return Retry.ElementNotFound();
            var element = elements[0];
            if (!element.Displayed || !element.Enabled) return Retry.ElementNotClickable();

            await Task.Run(element.Click);

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

        public async Task<Result> Wait(Func<IWebDriver, bool> condition)
        {
            Result wait()
            {
                try
                {
                    _wait.Until(condition);
                }
                catch (TimeoutException)
                {
                    return Result.Fail(new Stop("Page not loaded in 3 mins"));
                }
                return Result.Ok();
            }
            return await Task.Run(wait);
        }

        public async Task<Result> WaitPageLoaded()
        {
            static bool pageLoaded(IWebDriver driver) => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");
            var result = await Wait(pageLoaded);
            return result;
        }

        public async Task<Result> WaitPageChanged(string part)
        {
            bool pageChanged(IWebDriver driver) => driver.Url.Contains(part);
            Result result;
            result = await Wait(pageChanged);
            if (result.IsFailed) return result;
            result = await WaitPageLoaded();
            return result;
        }

        public async Task Close() => await Task.Run(_driver.Close);
    }
}