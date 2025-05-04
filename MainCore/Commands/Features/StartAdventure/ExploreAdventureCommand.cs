using MainCore.Commands.Base;

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ExploreAdventureCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            ILogService logService,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            if (!AdventureParser.CanStartAdventure(html)) return Skip.NoAdventure;

            var adventureButton = AdventureParser.GetAdventureButton(html);
            if (adventureButton is null) return Retry.ButtonNotFound("adventure");
            var logger = logService.GetLogger(command.AccountId);
            logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventureButton));

            static bool ContinueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = AdventureParser.GetContinueButton(doc);
                return continueButton is not null;
            }

            var result = await chromeBrowser.Click(By.XPath(adventureButton.XPath));
            if (result.IsFailed) return result;

            result = await chromeBrowser.Wait(ContinueShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}