using System.Web;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToBuildingCommand
    {
        public sealed record Command(int Location) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           CancellationToken cancellationToken
           )
        {
            var location = command.Location;

            var html = browser.Html;
            var node = GetBuilding(html, location);
            if (node is null) return Retry.NotFound($"{location}", "nodeBuilding");

            Result result;
            if (location > 18 && node.HasClass("g0"))
            {
                if (location == 40) // wall
                {
                    var currentUrl = new Uri(browser.CurrentUrl);
                    var host = currentUrl.GetLeftPart(UriPartial.Authority);
                    await browser.Navigate($"{host}/build.php?id={location}", cancellationToken);
                }
                else
                {
                    var css = $"#villageContent > div.buildingSlot.a{location} > svg > path";
                    result = await browser.Click(By.CssSelector(css));
                    if (result.IsFailed) return result;
                    result = await browser.WaitPageChanged("build.php", cancellationToken);
                    if (result.IsFailed) return result;
                }
            }
            else
            {
                if (location == 40) // wall
                {
                    var path = node.Descendants("path").FirstOrDefault();
                    if (path is null) return Retry.NotFound($"{location}", "wall bottom path");

                    var javascript = path.GetAttributeValue("onclick", "");
                    if (string.IsNullOrEmpty(javascript)) return Retry.NotFound($"{location}", "JavaScriptExecutor onclick wall");

                    var decodedJs = HttpUtility.HtmlDecode(javascript);

                    result = await browser.ExecuteJsScript(decodedJs);
                    if (result.IsFailed) return result;
                }
                else
                {
                    result = await browser.Click(By.XPath(node.XPath));
                    if (result.IsFailed) return result;
                }
                result = await browser.WaitPageChanged("build.php", cancellationToken);
                if (result.IsFailed) return result;
            }
            return Result.Ok();
        }

        private static HtmlNode GetBuilding(HtmlDocument doc, int location)
        {
            if (location < 19) return GetField(doc, location);
            return GetInfrastructure(doc, location);
        }

        private static HtmlNode GetField(HtmlDocument doc, int location)
        {
            var node = doc.DocumentNode
                   .Descendants("a")
                   .First(x => x.HasClass($"buildingSlot{location}"));
            return node;
        }

        private static HtmlNode GetInfrastructure(HtmlDocument doc, int location)
        {
            var tmpLocation = location - 18;
            var div = doc.DocumentNode
                .SelectSingleNode($"//*[@id='villageContent']/div[{tmpLocation}]");
            if (div is null) throw new NullReferenceException($"Cannot find //*[@id='villageContent']/div[{tmpLocation}]");
            return div;
        }
    }
}