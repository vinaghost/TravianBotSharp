using System.Web;

namespace MainCore.Features.Navigate;

[Handler]
public static partial class ToBuildingByLocationCommand
{
    public sealed record Command(IChromeBrowser Browser, int Location) : ICommand
    {
        public string Name => $"{nameof(ToBuildingByLocationCommand)} [{Description}]";
        private string Description => $"{Location}";
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, location) = command;
        Result result;
        if (location < 19)
        {
            var field = browser.CurrentPage.GetFieldButton(location);
            result = await browser.Click(field);
            if (result.IsFailed) return result;
        }
        else
        {
            var building = browser.CurrentPage.GetBuildingButton(location);

            var javascript = await building.GetAttributeAsync("onclick");
            var decodedJs = HttpUtility.HtmlDecode(javascript ?? "");

            result = await browser.ExecuteJsScript(decodedJs);
            if (result.IsFailed) return result;
        }

        result = await browser.Wait(browser.CurrentPage.GetBuildPanel());
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}