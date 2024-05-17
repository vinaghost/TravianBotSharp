using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartAdventure
{
    public class ToAdventurePageCommand : AdventureCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            var adventure = GetHeroAdventure(html);
            if (adventure is null) return Retry.ButtonNotFound("hero adventure");

            bool tableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var table = doc.DocumentNode
                    .Descendants("table")
                    .Any(x => x.HasClass("adventureList"));
                return table;
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath), "adventures", tableShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}