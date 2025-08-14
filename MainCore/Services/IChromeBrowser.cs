using OpenQA.Selenium.Chrome;

namespace MainCore.Services
{
    public interface IChromeBrowser
    {
        string CurrentUrl { get; }
        ChromeDriver Driver { get; }
        HtmlDocument Html { get; }
        ILogger Logger { get; set; }

        Task<Result> Click(By by, CancellationToken cancellationToken);

        Task Close();

        Task<Result> ExecuteJsScript(string javascript);

        Task<Result> Input(By by, string content, CancellationToken cancellationToken);

        Task<Result> Navigate(string url, CancellationToken cancellationToken);

        Task<Result> Refresh(CancellationToken cancellationToken);

        Task<string> Screenshot();

        Task Setup(ChromeSetting setting);

        Task Shutdown();

        Task<Result> Wait(Predicate<IWebDriver> condition, CancellationToken cancellationToken);

        Task<Result> WaitPageChanged(string part, CancellationToken cancellationToken);

        Task<Result> WaitPageChanged(string part, Predicate<IWebDriver> customCondition, CancellationToken cancellationToken);

        Task<Result> WaitPageLoaded(CancellationToken cancellationToken);
    }
}