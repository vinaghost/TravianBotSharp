using FluentResults;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Step.StartFarmlist
{
    [RegisterAsTransient]
    public class StartAllFarmListCommand : IStartAllFarmListCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public StartAllFarmListCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public Result Execute(AccountId accountId)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var startAllButton = _unitOfParser.FarmParser.GetStartAllButton(html);
            if (startAllButton is null)
            {
                return Result.Fail(new Retry("Cannot found start all button"));
            }

            var result = chromeBrowser.Click(By.XPath(startAllButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}