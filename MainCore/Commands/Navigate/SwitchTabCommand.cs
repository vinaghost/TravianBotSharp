namespace MainCore.Commands.Navigate
{
    public class SwitchTabCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, int index, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var count = CountTab(html);
            if (index > count) return Retry.OutOfIndexTab(index, count);
            var tab = GetTab(html, index);
            if (tab is null) return Retry.NotFound($"{index}", "tab");
            if (IsTabActive(tab)) return Result.Ok();

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var count = CountTab(doc);
                if (index > count) return false;
                var tab = GetTab(doc, index);
                if (tab is null) return false;
                if (!IsTabActive(tab)) return false;
                return true;
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(tab.XPath), tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetNavigationBar(HtmlDocument doc)
        {
            var navigationBar = doc.DocumentNode
             .Descendants("div")
             .FirstOrDefault(x => x.HasClass("contentNavi") && x.HasClass("subNavi"));
            return navigationBar;
        }

        private static IEnumerable<HtmlNode> GetTabs(HtmlDocument doc)
        {
            var navigationBar = GetNavigationBar(doc);
            if (navigationBar is null) return Enumerable.Empty<HtmlNode>();
            var tabs = navigationBar
                .Descendants("a")
                .Where(x => x.HasClass("tabItem"));
            return tabs;
        }

        private static int CountTab(HtmlDocument doc)
        {
            var count = GetTabs(doc)
                .Count();
            return count;
        }

        private static HtmlNode GetTab(HtmlDocument doc, int index)
        {
            var tab = GetTabs(doc)
                .ElementAt(index);
            return tab;
        }

        private static bool IsTabActive(HtmlNode node)
        {
            return node.HasClass("active");
        }
    }
}