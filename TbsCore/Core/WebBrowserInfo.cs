﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TravBotSharp.Files.Helpers;
using TravBotSharp.Files.Tasks.LowLevel;

namespace TravBotSharp.Files.Models.AccModels
{
    public class WebBrowserInfo
    {
        public ChromeDriver Driver { get; set; }
        public string CurrentUrl { get; set; }
        private Account acc;
        public HtmlAgilityPack.HtmlDocument Html { get; set; }

        public async Task InitSelenium(Account acc)
        {
            this.acc = acc;
            var access = await acc.Access.GetNewAccess();

            SetupChromeDriver(access, acc.AccInfo.Nickname, acc.AccInfo.ServerUrl);

            if(this.Html == null)
            {
                this.Html = new HtmlAgilityPack.HtmlDocument();
            }

            if (!string.IsNullOrEmpty(access.Proxy))
            {
                var checkproxy = new CheckProxy();
                await checkproxy.Execute(acc);
            }
        }

        private void SetupChromeDriver(Access access, string username, string server)
        {
            ChromeOptions options = new ChromeOptions();
            if (!string.IsNullOrEmpty(access.Proxy))
            {
                options.AddArgument($"--proxy-server={access.Proxy}:{access.ProxyPort}");
                options.AddArgument("ignore-certificate-errors");
            }
            if (!string.IsNullOrEmpty(access.UserAgent))
            {
                options.AddArgument("--user-agent=" + access.UserAgent);
            }

            // Make browser headless to preserve memory resources
            if(acc.Settings.HeadlessMode) options.AddArguments("headless");

            // Do not download images in order to preserve memory resources
            if (acc.Settings.DisableImages) options.AddArguments("--blink-settings=imagesEnabled=false");

            // Add browser caching
            var accFolder = IoHelperCore.GetCacheFolder(username, server, access.Proxy);
            var dir = Path.Combine(IoHelperCore.CachePath, accFolder);
            Directory.CreateDirectory(dir);
            options.AddArguments("user-data-dir=" + dir);

            // Disable message "Chrome is being controlled by automated test software"
            //options.AddArgument("--excludeSwitches=enable-automation"); // Doesn't work anymore

            // Hide command prompt
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            try
            {
                this.Driver = new ChromeDriver(service, options);

                // Set timeout
                this.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
            }
            catch(Exception e)
            {
                // TODO: log exception
            }
        }

        internal Dictionary<string, string> GetCookes()
        {
            var cookies = Driver.Manage().Cookies.AllCookies;
            var cookiesDir = new Dictionary<string, string>();
            for (int i = 0; i < cookies.Count; i++)
            {
                cookiesDir.Add(cookies[i].Name, cookies[i].Value);
            }
            return cookiesDir;
        }

        public async Task Navigate(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            this.CurrentUrl = url;
            int repeatCnt = 0;
            bool repeat;
            do
            {
                try
                {
                    // Will throw exception after timeout
                    this.Driver.Navigate().GoToUrl(url);
                    repeat = false;
                }
                catch (Exception e)
                {
                    repeat = true;
                    if (++repeatCnt >= 3 && !string.IsNullOrEmpty(acc.Access.GetCurrentAccess().Proxy))
                    {
                        // Change access
                        repeatCnt = 0;
                        var changeAccess = new ChangeAccess();
                        await changeAccess.Execute(acc);
                        await Task.Delay(AccountHelper.Delay() * 5);
                    }
                }
            }
            while(repeat);


            await Task.Delay(AccountHelper.Delay());
            if (!string.IsNullOrEmpty(acc.Access.GetCurrentAccess().Proxy))
            {
                // We are using proxy. Connection is probably slower -> additional delay.
                await Task.Delay(AccountHelper.Delay() * 2);
            }

            this.Html.LoadHtml(this.Driver.PageSource);
            await TaskExecutor.PageLoaded(acc);
        }

        public void Close()
        {
            try
            {
                this.Driver.Quit();
            }
            catch(Exception e)
            {
                Utils.log.Warning($"Exception occured when trying to close selenium driver. Acc {acc.AccInfo.Nickname}\n{e.Message}");
            }
        }
    }
}