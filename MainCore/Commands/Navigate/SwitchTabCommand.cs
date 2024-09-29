using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class SwitchTabCommand(DataService dataService) : CommandBase<int>(dataService)
    {
        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var count = BuildingTabParser.CountTab(html);
            if (Data > count) return Retry.OutOfIndexTab(Data, count);
            var tab = BuildingTabParser.GetTab(html, Data);
            if (tab is null) return Retry.NotFound($"{Data}", "tab");
            if (BuildingTabParser.IsTabActive(tab)) return Result.Ok();

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var count = BuildingTabParser.CountTab(doc);
                if (Data > count) return false;
                var tab = BuildingTabParser.GetTab(doc, Data);
                if (tab is null) return false;
                if (!BuildingTabParser.IsTabActive(tab)) return false;
                return true;
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(tab.XPath), tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}