using OpenQA.Selenium.Chrome;
using System.Runtime.CompilerServices;

namespace MainCore.Services
{
    public interface IChromeBrowser
    {
        string CurrentUrl { get; }
        ChromeDriver? Driver { get; }
        HtmlDocument Html { get; }
        ILogger Logger { get; set; }

        Task<Result> Click(IWebElement element, CancellationToken cancellationToken);

        Task Close();

        Task<Result> ExecuteJsScript(string javascript);

        Task<Result<IWebElement>> GetElement(Func<HtmlDocument, HtmlNode?> nodeGenerator, CancellationToken cancellationToken, [CallerArgumentExpression("nodeGenerator")] string? expression = null);

        Task<Result<IWebElement>> GetElement(By by, CancellationToken cancellationToken, [CallerArgumentExpression("by")] string? expression = null);

        Task<Result> Input(IWebElement element, string content, CancellationToken cancellationToken);

        Task<Result> Navigate(string url, CancellationToken cancellationToken);

        Task<Result> Refresh(CancellationToken cancellationToken);

        Task<string> Screenshot();

        Task Setup(ChromeSetting setting);

        Task Shutdown();

        Task<Result> Wait(Predicate<IWebDriver> condition, CancellationToken cancellationToken, [CallerArgumentExpression("condition")] string? expression = null);
    }
}