using System.Web;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToBuildingCommand
    {
        public sealed record Command(AccountId AccountId, int Location) : ICustomCommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeManager chromeManager,
           CancellationToken cancellationToken
           )
        {
            var (accountId, location) = command;
            var browser = chromeManager.Get(accountId);
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
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    result = await browser.WaitPageChanged("build.php", cancellationToken);
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

                    result = await browser.ExecuteJsScript(decodedJs);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                else
                {
                    result = await browser.Click(By.XPath(node.XPath));
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                result = await browser.WaitPageChanged("build.php", cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
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