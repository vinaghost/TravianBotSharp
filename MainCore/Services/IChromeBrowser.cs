using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MainCore.Services
{
    public interface IChromeBrowser
    {
        string CurrentUrl { get; }
        ChromeDriver Driver { get; }
        HtmlDocument Html { get; }

        Task<Result> Click(By by);

        Task Close();

        Task<Result> InputTextbox(By by, string content);

        bool IsOpen();

        Task<Result> Navigate(string url = null);

        Task<Result> Setup(ChromeSetting setting);

        Task Shutdown();

        Task<Result> Wait(Func<IWebDriver, bool> condition);

        Task<Result> WaitPageChanged(string part);

        Task<Result> WaitPageLoaded();
    }
}