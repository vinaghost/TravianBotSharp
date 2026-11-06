using System.Web;

namespace MainCore.Commands.Navigate
{
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
                var field = BuildingLayoutParser.GetFieldButton(browser.CurrentPage, location);
                result = await browser.Click(field);
                if (result.IsFailed) return result;
            }
            else
            {
                var building = BuildingLayoutParser.GetBuildingButton(browser.CurrentPage, location);

                var javascript = await building.GetAttributeAsync("onclick");
                var decodedJs = HttpUtility.HtmlDecode(javascript ?? "");

                result = await browser.ExecuteJsScript(decodedJs);
                if (result.IsFailed) return result;
            }

            result = await browser.WaitPageChanged("build");
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}