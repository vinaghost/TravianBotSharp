using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartAdventure
{
    [RegisterScoped<ExploreAdventureCommand>]
    public class ExploreAdventureCommand(DataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            if (!AdventureParser.CanStartAdventure(html)) return Skip.NoAdventure;

            var adventureButton = AdventureParser.GetAdventureButton(html);
            if (adventureButton is null) return Retry.ButtonNotFound("adventure");

            var logger = _dataService.Logger;
            logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventureButton));

            static bool continueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = AdventureParser.GetContinueButton(doc);
                return continueButton is not null;
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventureButton.XPath), continueShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}