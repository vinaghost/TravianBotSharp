using FluentResults;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    [RegisterAsTransient]
    public class UpgradeCommand : IUpgradeCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public UpgradeCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Execute(AccountId accountId)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var button = _unitOfParser.UpgradeBuildingParser.GetUpgradeButton(html);
            if (button is null) return Result.Fail(Retry.ButtonNotFound("upgrade"));

            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("dorf");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}