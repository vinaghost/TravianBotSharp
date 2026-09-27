using System.Web;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class ToBuildingByLocationCommand(IChromeBrowser browser)
    {
        public sealed record Command(int Location) : ICommand;

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var location = command.Location;
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
            if (location == 40) // wall
            {
                var node = page.Locator(".village2 div.buildingSlot.a40.top svg path");
                return node;
            }

            var div = page.Locator($".village2 div.buildingSlot.a{location} svg path");
            return div.First;
        }
    }
}