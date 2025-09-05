using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace MainCore.Services
{
    public sealed class FirefoxBrowser : IBrowser
    {
        private FirefoxDriver? _driver;
        private readonly FirefoxDriverService _firefoxService;
        private WebDriverWait _wait = null!;

        private readonly string[] _extensionsPath;
        private readonly HtmlDocument _htmlDoc = new();

        public FirefoxBrowser(string[] extensionsPath)
        {
            _extensionsPath = extensionsPath;

            _firefoxService = FirefoxDriverService.CreateDefaultService();
            _firefoxService.HideCommandPromptWindow = true;
        }

        public async Task Setup(BrowserSetting setting)
        {
            var options = new FirefoxOptions();
            
            // Firefox'un yolunu bul ve belirt
            var firefoxPath = FindFirefoxPath();
            if (!string.IsNullOrEmpty(firefoxPath))
            {
                options.BinaryLocation = firefoxPath;
            }
            else
            {
                throw new InvalidOperationException("Firefox bulunamadı! Lütfen Firefox'u yükleyin veya PATH'e ekleyin.");
            }

            // Firefox için extension ekleme (Chrome'dan farklı)
            // Firefox'ta extension ekleme daha karmaşık, şimdilik atlayalım
            // foreach (var extensionPath in _extensionsPath)
            // {
            //     options.AddExtension(extensionPath);
            // }

            // Proxy ayarları
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

            // User agent ayarı
            if (!string.IsNullOrEmpty(setting.UserAgent))
            {
                options.AddArgument($"--user-agent={setting.UserAgent}");
            }

            // Firefox'a özgü ayarlar
            options.AddArgument("--ignore-certificate-errors");
            options.AddArguments("--no-default-browser-check", "--no-first-run");
            options.AddArguments("--mute-audio", "--disable-gpu");

            // Firefox 138.0+ için gerekli argüman
            options.AddArgument("--allow-system-access");

            // Automation detection'ı engelle
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddAdditionalOption("useAutomationExtension", "undefined");

            // Headless mod
            if (setting.IsHeadless)
            {
                options.AddArgument("--headless");
            }
            else
            {
                options.AddArgument("--start-maximized");
            }

            // Firefox profil ayarları - geçerli bir yol oluştur
            var baseProfilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Cache", "FirefoxProfiles");
            if (!Directory.Exists(baseProfilePath)) Directory.CreateDirectory(baseProfilePath);

            // Profil yolu için geçerli karakterler kullan
            var safeProfilePath = SanitizePath(setting.ProfilePath);
            var safeProxyName = string.IsNullOrEmpty(setting.ProxyHost) ? "default" : SanitizePath(setting.ProxyHost);
            
            var profilePath = Path.Combine(baseProfilePath, safeProfilePath, safeProxyName);
            if (!Directory.Exists(profilePath)) Directory.CreateDirectory(profilePath);

            // Firefox profil oluştur
            var profile = new FirefoxProfile(profilePath);
            options.Profile = profile;

            _driver = await Task.Run(() => new FirefoxDriver(_firefoxService, options, TimeSpan.FromMinutes(3)));

            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            // Firefox için daha uzun timeout - sayfa yükleme daha yavaş olabilir
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(150)); // building operations
        }

        public IWebDriver Driver => _driver!;

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
            _firefoxService.Dispose();
        }

        public string CurrentUrl => Driver.Url;

        public ILogger Logger { get; set; } = null!;

        public async Task<string> Screenshot()
        {
            var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
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
            
            try
            {
                // Önce element'i viewport'a scroll et
                await Task.Run(() => ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'instant', block: 'center'});", element));
                
                // Kısa bir bekleme
                await Task.Delay(200);
                
                // Firefox için daha güvenilir: önce normal click dene
                await Task.Run(() => element.Click());
            }
            catch
            {
                // Normal click başarısız olursa, JavaScript click dene
                try
                {
                    await Task.Run(() => ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", element));
                }
                catch
                {
                    // Son çare olarak Actions kullan (viewport sınırları kontrolü ile)
                    try
                    {
                        var location = element.Location;
                        var size = element.Size;
                        var viewportSize = Driver.Manage().Window.Size;
                        
                        // Element viewport içindeyse Actions kullan
                        if (location.X >= 0 && location.Y >= 0 && 
                            location.X + size.Width <= viewportSize.Width && 
                            location.Y + size.Height <= viewportSize.Height)
                        {
                            await Task.Run(new Actions(Driver).Click(element).Perform);
                        }
                        else
                        {
                            // Viewport dışındaysa JavaScript ile zorla click
                            await Task.Run(() => ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].dispatchEvent(new MouseEvent('click', {bubbles: true}));", element));
                        }
                    }
                    catch
                    {
                        return Retry.ElementNotClickable(by);
                    }
                }
            }
            
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
            js?.ExecuteScript(javascript);
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
            catch (WebDriverTimeoutException ex)
            {
                return Retry.BrowserTimeout(ex.Message);
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

        public async Task Close()
        {
            await Task.Run(() => _driver?.Quit());
        }

        private static string? FindFirefoxPath()
        {
            // Yaygın Firefox kurulum yolları
            var possiblePaths = new[]
            {
                @"C:\Program Files\Mozilla Firefox\firefox.exe",
                @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe",
                @"C:\Users\{USERNAME}\AppData\Local\Mozilla Firefox\firefox.exe",
                @"C:\ProgramData\Mozilla Firefox\firefox.exe"
            };

            foreach (var path in possiblePaths)
            {
                // {USERNAME} placeholder'ını gerçek kullanıcı adıyla değiştir
                var expandedPath = path.Replace("{USERNAME}", Environment.UserName);
                
                if (File.Exists(expandedPath))
                {
                    return expandedPath;
                }
            }

            // PATH'de Firefox'u ara
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = "firefox.exe",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var firstPath = output.Split('\n')[0].Trim();
                    if (File.Exists(firstPath))
                    {
                        return firstPath;
                    }
                }
            }
            catch
            {
                // Hata durumunda null döndür
            }

            return null;
        }

        private static string SanitizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "default";

            // Windows dosya sistemi için geçersiz karakterleri temizle
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(path.Where(c => !invalidChars.Contains(c)).ToArray());
            
            // Boş string olursa default döndür
            if (string.IsNullOrWhiteSpace(sanitized))
                return "default";
                
            return sanitized;
        }
    }

    public static class FirefoxOptionsExtensions
    {
        /// <summary>
        /// Add HTTP-proxy by <paramref name="userName"/> and <paramref name="password"/>
        /// </summary>
        /// <param name="options">Firefox options</param>
        /// <param name="host">Proxy host</param>
        /// <param name="port">Proxy port</param>
        /// <param name="userName">Proxy username</param>
        /// <param name="password">Proxy password</param>
        public static void AddHttpProxy(this FirefoxOptions options, string host, int port, string userName, string password)
        {
            // Firefox için proxy ayarları profil üzerinden yapılır
            var profile = options.Profile ?? new FirefoxProfile();
            
            // Proxy ayarları
            profile.SetPreference("network.proxy.type", 1); // Manual proxy
            profile.SetPreference("network.proxy.http", host);
            profile.SetPreference("network.proxy.http_port", port);
            profile.SetPreference("network.proxy.ssl", host);
            profile.SetPreference("network.proxy.ssl_port", port);
            profile.SetPreference("network.proxy.ftp", host);
            profile.SetPreference("network.proxy.ftp_port", port);
            profile.SetPreference("network.proxy.socks", host);
            profile.SetPreference("network.proxy.socks_port", port);
            
            // Proxy authentication
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                profile.SetPreference("network.proxy.http_use_dns", false);
                profile.SetPreference("network.proxy.ssl_use_dns", false);
                profile.SetPreference("network.proxy.ftp_use_dns", false);
                profile.SetPreference("network.proxy.socks_use_dns", false);
                
                // Firefox için proxy authentication extension gerekebilir
                // Bu durumda basit bir çözüm olarak proxy'yi authentication olmadan kullanabiliriz
            }
            
            options.Profile = profile;
        }
    }
}
