using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace MainCore.Services
{
    public interface IChromeBrowser
    {
        IPage CurrentPage { get; }
        string CurrentUrl { get; }
        ILogger Logger { get; set; }

        Task<Result> Click(ILocator locator, [CallerArgumentExpression(nameof(locator))] string? expression = null);

        Task<Result> ExecuteJsScript(string javascript);

        Task<Result> Input(ILocator locator, string content, [CallerArgumentExpression(nameof(locator))] string? expression = null);

        Task<Result> Navigate(string url);

        Task<Result> Refresh();

        Task<string> Screenshot();

        Task Setup(ChromeSetting setting);

        Task Shutdown();

        Task<Result> Wait(ILocator locator, [CallerArgumentExpression(nameof(locator))] string? expression = null);

        Task<Result> Wait(ILocator locator, string condition, [CallerArgumentExpression(nameof(locator))] string? expression = null);

        Task<Result> WaitPageChanged(string url);
    }
}