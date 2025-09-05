namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class SwitchTabCommand
    {
        public sealed record Command(int TabIndex) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IBrowser browser,
           CancellationToken cancellationToken
           )
        {
            return await SwitchTab(browser, command.TabIndex, cancellationToken);
        }

        public static async ValueTask<Result> SwitchTab(
            IBrowser browser,
            int tabIndex,
            CancellationToken cancellationToken)
        {
            var count = BuildingTabParser.CountTab(browser.Html);
            if (tabIndex > count) return Retry.OutOfIndexTab(tabIndex, count);
            var tab = BuildingTabParser.GetTab(browser.Html, tabIndex);
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
            result = await browser.Click(By.XPath(tab.XPath));
            if (result.IsFailed) return result;
            result = await browser.Wait(tabActived, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}
