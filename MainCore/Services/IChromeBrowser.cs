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
        string EndpointAddress { get; }

        Task<Result> Click(By by);

        Task Close();
        long[] GetProduction();
        long[] GetStorage();
        TimeSpan GetTimeEnoughResource(long[] required);
        Task<Result> InputTextbox(By by, string content);

        bool IsOpen();

        Task<Result> Navigate(string url, CancellationToken cancellationToken);

        Task<Result> Refresh(CancellationToken cancellationToken);

        Task<Result> Setup(ChromeSetting setting);

        Task Shutdown();

        Task<Result> Wait(Func<IWebDriver, bool> condition, CancellationToken cancellationToken);

        Task<Result> WaitPageChanged(string part, CancellationToken cancellationToken);

        Task<Result> WaitPageLoaded(CancellationToken cancellationToken);
    }
}