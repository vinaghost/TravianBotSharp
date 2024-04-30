using HtmlAgilityPack;
using MainCore.Commands.Base;

namespace MainCore.Commands.Navigate.ToAdventurePageCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TravianOfficial)]
    public class TravianOfficial : ICommandHandler<ToAdventurePageCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public TravianOfficial(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToAdventurePageCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var adventure = _unitOfParser.HeroParser.GetHeroAdventure(html);
            if (adventure is null) return Retry.ButtonNotFound("hero adventure");

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("adventures", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool tableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var table = doc.DocumentNode
                    .Descendants("table")
                    .Where(x => x.HasClass("adventureList"))
                    .Any();
                return table;
            };

            result = await chromeBrowser.Wait(tableShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}