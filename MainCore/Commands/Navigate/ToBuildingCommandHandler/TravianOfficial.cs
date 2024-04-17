using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;
using System.Web;

namespace MainCore.Commands.Navigate.ToBuildingCommandHandler
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : ICommandHandler<ToBuildingCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public TravianOfficial(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToBuildingCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var node = _unitOfParser.BuildingParser.GetBuilding(html, command.Location);
            if (node is null) return Retry.NotFound($"{command.Location}", "nodeBuilding");

            Result result;
            if (command.Location > 18 && node.HasClass("g0"))
            {
                if (command.Location == 40) // wall
                {
                    var currentUrl = new Uri(chromeBrowser.CurrentUrl);
                    var host = currentUrl.GetLeftPart(UriPartial.Authority);
                    result = await chromeBrowser.Navigate($"{host}/build.php?id={command.Location}", cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                else
                {
                    var css = $"#villageContent > div.buildingSlot.a{command.Location} > svg > path";
                    result = await chromeBrowser.Click(By.CssSelector(css));
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
            }
            else
            {
                if (command.Location == 40) // wall
                {
                    var path = node.Descendants("path").FirstOrDefault();
                    if (path is null) return Retry.NotFound($"{command.Location}", "wall bottom path");

                    var javascript = path.GetAttributeValue("onclick", "");
                    if (string.IsNullOrEmpty(javascript)) return Retry.NotFound($"{command.Location}", "JavaScriptExecutor onclick wall");

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