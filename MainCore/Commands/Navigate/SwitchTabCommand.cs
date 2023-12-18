using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Navigate
{
    public class SwitchTabCommand : ByAccountIdBase, ICommand
    {
        public int Index { get; }

        public SwitchTabCommand(AccountId accountId, int index) : base(accountId)
        {
            Index = index;
        }
    }

    [RegisterAsTransient]
    public class SwitchTabCommandHandler : ICommandHandler<SwitchTabCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public SwitchTabCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(SwitchTabCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var count = _unitOfParser.NavigationTabParser.CountTab(html);
            if (command.Index > count) return Result.Fail(new Retry($"Found {count} tabs but need tab {command.Index} active"));
            var tab = _unitOfParser.NavigationTabParser.GetTab(html, command.Index);
            if (tab is null) return Result.Fail(Retry.NotFound($"{command.Index}", "tab"));
            if (_unitOfParser.NavigationTabParser.IsTabActive(tab)) return Result.Ok();

            Result result;
            result = await chromeBrowser.Click(By.XPath(tab.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var count = _unitOfParser.NavigationTabParser.CountTab(doc);
                if (command.Index > count) return false;
                var tab = _unitOfParser.NavigationTabParser.GetTab(doc, command.Index);
                if (tab is null) return false;
                if (!_unitOfParser.NavigationTabParser.IsTabActive(tab)) return false;
                return true;
            };

            result = await chromeBrowser.Wait(tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}