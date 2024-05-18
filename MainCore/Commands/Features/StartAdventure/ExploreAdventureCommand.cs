using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartAdventure
{
    public class ExploreAdventureCommand : AdventureCommand
    {
        private readonly ILogService _logService;

        public ExploreAdventureCommand(ILogService logService = null)
        {
            _logService = logService ?? Locator.Current.GetService<ILogService>();
        }

        public async Task<Result> Execute(AccountId accountId, IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            if (!CanStartAdventure(html)) return Skip.NoAdventure;

            var adventure = GetAdventure(html);
            if (adventure is null) return Retry.ButtonNotFound("adventure place");

            var logger = _logService.GetLogger(accountId);
            logger.Information("Start adventure {Adventure}", GetAdventureInfo(adventure));
            bool continueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = GetContinueButton(doc);
                return continueButton is not null;
            }
            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath), continueShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetAdventure(HtmlDocument doc)
        {
            var adventures = doc.GetElementbyId("heroAdventure");
            if (adventures is null) return null;

            var tbody = adventures.Descendants("tbody").FirstOrDefault();
            if (tbody is null) return null;

            var tr = tbody.Descendants("tr").FirstOrDefault();
            if (tr is null) return null;
            var button = tr.Descendants("button").FirstOrDefault();
            return button;
        }

        private static HtmlNode GetContinueButton(HtmlDocument doc)
        {
            var button = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("continue"));
            return button;
        }

        private static string GetAdventureInfo(HtmlNode node)
        {
            var difficult = GetAdventureDifficult(node);
            var coordinates = GetAdventureCoordinates(node);

            return $"{difficult} - {coordinates}";
        }

        private static string GetAdventureDifficult(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();
            if (tdList.Length < 3) return "unknown";
            var iconDifficulty = tdList[3].FirstChild;
            return iconDifficulty.GetAttributeValue("alt", "unknown");
        }

        private static string GetAdventureCoordinates(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();
            if (tdList.Length < 2) return "[~|~]";
            return tdList[1].InnerText;
        }
    }
}