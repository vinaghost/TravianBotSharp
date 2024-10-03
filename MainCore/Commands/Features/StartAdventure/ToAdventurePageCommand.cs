using MainCore.Commands.Abstract;
using MainCore.Parsers;

namespace MainCore.Commands.Features.StartAdventure
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToAdventurePageCommand(DataService dataService) : CommandBase(dataService)
    {
        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var adventure = AdventureParser.GetHeroAdventureButton(html);
            if (adventure is null) return Retry.ButtonNotFound("hero adventure");

            static bool tableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return AdventureParser.IsAdventurePage(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath), "adventures", tableShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}