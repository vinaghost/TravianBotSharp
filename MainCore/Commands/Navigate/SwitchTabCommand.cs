using HtmlAgilityPack;

namespace MainCore.Commands.Navigate
{
    public class SwitchTabCommand : ICommand
    {
        public int Index { get; }
        public IChromeBrowser ChromeBrowser { get; }

        public SwitchTabCommand(IChromeBrowser chromeBrowser, int index)
        {
            Index = index;
            ChromeBrowser = chromeBrowser;
        }
    }

    [RegisterAsTransient]
    public class SwitchTabCommandHandler : ICommandHandler<SwitchTabCommand>
    {
        private readonly INavigationTabParser _navigationTabParser;

        public SwitchTabCommandHandler(INavigationTabParser navigationTabParser)
        {
            _navigationTabParser = navigationTabParser;
        }

        public async Task<Result> Handle(SwitchTabCommand request, CancellationToken cancellationToken)
        {
            var chromeBrowser = request.ChromeBrowser;
            var index = request.Index;

            var html = chromeBrowser.Html;
            var count = _navigationTabParser.CountTab(html);
            if (index > count) return Retry.OutOfIndexTab(index, count);
            var tab = _navigationTabParser.GetTab(html, index);
            if (tab is null) return Retry.NotFound($"{index}", "tab");
            if (_navigationTabParser.IsTabActive(tab)) return Result.Ok();

            Result result;
            result = await chromeBrowser.Click(By.XPath(tab.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var count = _navigationTabParser.CountTab(doc);
                if (index > count) return false;
                var tab = _navigationTabParser.GetTab(doc, index);
                if (tab is null) return false;
                if (!_navigationTabParser.IsTabActive(tab)) return false;
                return true;
            };
            result = await chromeBrowser.Wait(tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}