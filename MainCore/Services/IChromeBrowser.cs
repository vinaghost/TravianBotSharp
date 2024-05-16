using MainCore.Common.Models;
using OpenQA.Selenium.Chrome;

namespace MainCore.Services
{
    public interface IChromeBrowser
    {
        string CurrentUrl { get; }
        ChromeDriver Driver { get; }
        HtmlDocument Html { get; }
        string EndpointAddress { get; }

        Task<Result> Click(By by, CancellationToken cancellationToken);

        Task<Result> Click(By by, string url, CancellationToken cancellationToken);

        Task<Result> Click(By by, Func<IWebDriver, bool> condition, CancellationToken cancellationToken);

        Task<Result> Click(By by, string url, Func<IWebDriver, bool> condition, CancellationToken cancellationToken);

        Task Close();

        Task<Result> ExecuteJsScript(string javascript, string url, CancellationToken cancellationToken);

        Task<Result> InputTextbox(By by, string content);

        Task<Result> Navigate(string url, CancellationToken cancellationToken);

        Task<Result> Refresh(CancellationToken cancellationToken);

        Task<Result> Setup(ChromeSetting setting);

        Task Shutdown();

        Task<Result> Wait(Func<IWebDriver, bool> condition, CancellationToken cancellationToken);

        Task<Result> WaitPageChanged(string part, CancellationToken cancellationToken);

        Task<Result> WaitPageLoaded(CancellationToken cancellationToken);
    }
}