using HtmlAgilityPack;

namespace MainCore.Commands.Features.StartAdventure
{
    public class ToAdventurePageCommand : ICommand
    {
        public ToAdventurePageCommand(IChromeBrowser chromeBrowser)
        {
            ChromeBrowser = chromeBrowser;
        }

        public IChromeBrowser ChromeBrowser { get; }
    }

    public class ToAdventurePageCommandHandler : ICommandHandler<ToAdventurePageCommand>
    {
        private readonly IHeroParser _heroParser;

        public ToAdventurePageCommandHandler(IHeroParser heroParser)
        {
            _heroParser = heroParser;
        }

        public async Task<Result> Handle(ToAdventurePageCommand request, CancellationToken cancellationToken)
        {
            var chromeBrowser = request.ChromeBrowser;
            var html = chromeBrowser.Html;

            var adventure = _heroParser.GetHeroAdventure(html);
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