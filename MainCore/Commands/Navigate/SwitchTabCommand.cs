using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    [RegisterScoped<SwitchTabCommand>]
    public class SwitchTabCommand(IDataService dataService) : CommandBase(dataService), ICommand<int>
    {
        public async Task<Result> Execute(int tabIndex, CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var count = BuildingTabParser.CountTab(html);
            if (tabIndex > count) return Retry.OutOfIndexTab(tabIndex, count);
            var tab = BuildingTabParser.GetTab(html, tabIndex);
            if (tab is null) return Retry.NotFound($"{tabIndex}", "tab");
            if (BuildingTabParser.IsTabActive(tab)) return Result.Ok();

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var count = BuildingTabParser.CountTab(doc);
                if (tabIndex > count) return false;
                var tab = BuildingTabParser.GetTab(doc, tabIndex);
                if (tab is null) return false;
                if (!BuildingTabParser.IsTabActive(tab)) return false;
                return true;
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(tab.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.Wait(tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}