using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartAdventure
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ExploreAdventureCommand(DataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            if (!AdventureParser.CanStartAdventure(html)) return Skip.NoAdventure;

            var adventure = AdventureParser.GetAdventure(html);
            if (adventure is null) return Retry.ButtonNotFound("adventure place");

            var logger = _dataService.Logger;
            logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventure));

            static bool continueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = AdventureParser.GetContinueButton(doc);
                return continueButton is not null;
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath), continueShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}