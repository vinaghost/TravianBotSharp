using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Navigate.ToBuildingCommandHandler
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : ICommandHandler<ToBuildingCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public TravianOfficial(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToBuildingCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var node = _unitOfParser.BuildingParser.GetBuilding(html, command.Location);
            if (node is null) return Result.Fail(Retry.NotFound($"{command.Location}", "nodeBuilding"));

            Result result;
            if (command.Location > 18 && node.HasClass("g0"))
            {
                var css = $"#villageContent > div.buildingSlot.a{command.Location} > svg > path";
                result = await chromeBrowser.Click(By.CssSelector(css));
            }
            else
            {
                result = await chromeBrowser.Click(By.XPath(node.XPath));
            }
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}