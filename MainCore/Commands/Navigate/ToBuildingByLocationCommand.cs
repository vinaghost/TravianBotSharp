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
                var field = GetField(browser.CurrentPage, location);
                result = await browser.Click(field);
                if (result.IsFailed) return result;
            }
            else
            {
                var building = GetInfrastructure(browser.CurrentPage, location);

                var javascript = await building.GetAttributeAsync("onclick");
                var decodedJs = HttpUtility.HtmlDecode(javascript ?? "");

                result = await browser.ExecuteJsScript(decodedJs);
                if (result.IsFailed) return result;
            }

            result = await browser.WaitPageChanged("build");
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static ILocator GetField(IPage page, int location)
        {
            var node = page.Locator($".village1 a.buildingSlot{location}");
            return node;
        }

        private static ILocator GetInfrastructure(IPage page, int location)
        {
            var div = page.Locator($".village2 div.buildingSlot.a{location} svg path");
            return div.First;
        }
    }
}