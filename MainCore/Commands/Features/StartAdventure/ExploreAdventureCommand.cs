#pragma warning disable S1172

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ExploreAdventureCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;

            if (!AdventureParser.CanStartAdventure(html)) return Skip.NoAdventure;

            var adventureButton = AdventureParser.GetAdventureButton(html);
            if (adventureButton is null) return Retry.ButtonNotFound("adventure");
            logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventureButton));

            static bool ContinueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = AdventureParser.GetContinueButton(doc);
                return continueButton is not null;
            }

            var result = await browser.Click(By.XPath(adventureButton.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(ContinueShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}