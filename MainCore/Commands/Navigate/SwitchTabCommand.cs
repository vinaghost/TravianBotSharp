﻿using MainCore.Constraints;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class SwitchTabCommand
    {
        public sealed record Command(AccountId AccountId, int TabIndex) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           CancellationToken cancellationToken
           )
        {
            var (accountId, tabIndex) = command;

            var html = browser.Html;
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
            result = await browser.Click(By.XPath(tab.XPath));
            if (result.IsFailed) return result;
            result = await browser.Wait(tabActived, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}