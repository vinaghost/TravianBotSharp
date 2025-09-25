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
            if (!AdventureParser.CanStartAdventure(browser.Html)) return Skip.Error.WithError("No adventure available");

            var adventureButton = AdventureParser.GetAdventureButton(browser.Html);
            if (adventureButton is null) return Retry.Error.WithError($"Failed to find adventure button");

            logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventureButton));

            var (_, isFailed, element, errors) = await browser.GetElement(By.XPath(adventureButton.XPath), cancellationToken);
            if (isFailed) return Result.Fail(errors).WithError($"Failed to find adventure button [{adventureButton.XPath}]");

            var result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            static bool ContinueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = AdventureParser.GetContinueButton(doc);
                return continueButton is not null;
            }
            result = await browser.Wait(ContinueShow, cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}