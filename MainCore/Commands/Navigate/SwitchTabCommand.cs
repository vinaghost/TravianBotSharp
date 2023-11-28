using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Navigate
{
    [RegisterAsTransient]
    public class SwitchTabCommand : ISwitchTabCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public SwitchTabCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Execute(AccountId accountId, int index)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var count = _unitOfParser.NavigationTabParser.CountTab(html);
            if (index > count) return Result.Fail(new Retry($"Found {count} tabs but need tab {index} active"));
            var tab = _unitOfParser.NavigationTabParser.GetTab(html, index);
            if (_unitOfParser.NavigationTabParser.IsTabActive(tab)) return Result.Ok();

            Result result;
            result = await chromeBrowser.Click(By.XPath(tab.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var count = _unitOfParser.NavigationTabParser.CountTab(doc);
                if (index > count) return false;
                var tab = _unitOfParser.NavigationTabParser.GetTab(doc, index);
                if (!_unitOfParser.NavigationTabParser.IsTabActive(tab)) return false;
                return true;
            };

            result = await chromeBrowser.Wait(tabActived);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}