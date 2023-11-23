using FluentResults;
using HtmlAgilityPack;
using MainCore.DTO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MainCore.Services
{
    public interface IChromeBrowser
    {
        string CurrentUrl { get; }
        ChromeDriver Driver { get; }
        HtmlDocument Html { get; }

        Result Click(By by);

        void Close();

        Result InputTextbox(By by, string content);

        bool IsOpen();

        Result Navigate(string url = null);

        Result Setup(AccountDto account, AccessDto access);

        void Shutdown();

        Result Wait(Func<IWebDriver, bool> condition);

        Result WaitPageChanged(string part);

        Result WaitPageLoaded();
    }
}