using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;

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
            if (command.Index > count) return Retry.OutOfIndexTab(command.Index, count);
            var tab = _unitOfParser.NavigationTabParser.GetTab(html, command.Index);
            if (tab is null) return Retry.NotFound($"{command.Index}", "tab");
            if (_unitOfParser.NavigationTabParser.IsTabActive(tab)) return Result.Ok();

            Result result;
            result = await chromeBrowser.Click(By.XPath(tab.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

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
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}