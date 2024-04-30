using System.Web;

namespace MainCore.Commands.Navigate
{
    public class ToBuildingCommand : ICommand
    {
        public ToBuildingCommand(int location, IChromeBrowser chromeBrowser)
        {
            Location = location;
            ChromeBrowser = chromeBrowser;
        }

        public int Location { get; }
        public IChromeBrowser ChromeBrowser { get; }
    }

    public class ToBuildingCommandHandler : ICommandHandler<ToBuildingCommand>
    {
        private readonly IBuildingParser _buildingParser;

        public ToBuildingCommandHandler(IBuildingParser buildingParser)
        {
            _buildingParser = buildingParser;
        }

        public async Task<Result> Handle(ToBuildingCommand request, CancellationToken cancellationToken)
        {
            var chromeBrowser = request.ChromeBrowser;
            var location = request.Location;

            var html = chromeBrowser.Html;
            var node = _buildingParser.GetBuilding(html, location);
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
                    result = await chromeBrowser.Click(By.CssSelector(css));
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
                    var js = chromeBrowser.Driver as IJavaScriptExecutor;
                    js.ExecuteScript(decodedJs);

                    result = await chromeBrowser.WaitPageChanged("build.php", cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                else
                {
                    result = await chromeBrowser.Click(By.XPath(node.XPath));
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
            }
            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}