#pragma warning disable S1172

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ToAdventurePageCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var adventure = AdventureParser.GetHeroAdventureButton(browser.Html);
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