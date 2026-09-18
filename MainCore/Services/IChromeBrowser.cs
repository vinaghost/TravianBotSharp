using Microsoft.Playwright;

namespace MainCore.Services
{
    public interface IChromeBrowser
    {
        string CurrentUrl { get; }
        ILogger Logger { get; set; }

        Task<Result> Click(ILocator locator);

        Task<Result> ExecuteJsScript(string javascript);

        Task<HtmlDocument> GetHtml();

        Task<Result> Input(ILocator locator, string content);

        Task<Result> Navigate(string url);

        Task<Result> Refresh();

        Task<string> Screenshot();

        Task Setup(ChromeSetting setting);

        Task Shutdown();

        Task<Result> WaitPageChanged(string url);
    }
}