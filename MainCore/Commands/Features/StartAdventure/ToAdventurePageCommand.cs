namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ToAdventurePageCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {

            var html = browser.Html;

            var adventure = AdventureParser.GetHeroAdventureButton(html);
            if (adventure is null) return Retry.ButtonNotFound("hero adventure");

            static bool TableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return AdventureParser.IsAdventurePage(doc);
            }

            var result = await browser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("adventures", TableShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}