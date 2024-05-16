using System.Web;

namespace MainCore.Commands.Navigate
{
    public class ToBuildingCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, int location, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = GetBuilding(html, location);
            if (node is null) return Retry.NotFound($"{location}", "nodeBuilding");

            Result result;
            if (location > 18 && node.HasClass("g0"))
            {
                if (location == 40) // wall
                {
                    var currentUrl = new Uri(chromeBrowser.CurrentUrl);
                    var host = currentUrl.GetLeftPart(UriPartial.Authority);
                    result = await chromeBrowser.Navigate($"{host}/build.php?id={location}", cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                else
                {
                    var css = $"#villageContent > div.buildingSlot.a{location} > svg > path";
                    result = await chromeBrowser.Click(By.CssSelector(css), "build.php", cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
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

                    result = await chromeBrowser.ExecuteJsScript(decodedJs, "build.php", cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                else
                {
                    result = await chromeBrowser.Click(By.XPath(node.XPath), "build.php", cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
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
                   .FirstOrDefault(x => x.HasClass($"buildingSlot{location}"));
            return node;
        }

        private static HtmlNode GetInfrastructure(HtmlDocument doc, int location)
        {
            var tmpLocation = location - 18;
            var div = doc.DocumentNode
                .SelectSingleNode($"//*[@id='villageContent']/div[{tmpLocation}]");

            return div;
        }
    }
}