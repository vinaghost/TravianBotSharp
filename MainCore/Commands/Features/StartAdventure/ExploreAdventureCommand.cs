using HtmlAgilityPack;

namespace MainCore.Commands.Features.StartAdventure
{
    public class ExploreAdventureCommand : ICommand
    {
        public ExploreAdventureCommand(IChromeBrowser chromeBrowser)
        {
            ChromeBrowser = chromeBrowser;
        }

        public IChromeBrowser ChromeBrowser { get; }
    }

    public class ExploreAdventureCommandHandler : ICommandHandler<ExploreAdventureCommand>
    {
        private readonly IHeroParser _heroParser;

        public ExploreAdventureCommandHandler(IHeroParser heroParser)
        {
            _heroParser = heroParser;
        }

        public async Task<Result> Handle(ExploreAdventureCommand request, CancellationToken cancellationToken)
        {
            var chromeBrowser = request.ChromeBrowser;
            var html = chromeBrowser.Html;

            var adventure = _heroParser.GetAdventure(html);
            if (adventure is null) return Retry.ButtonNotFound("adventure place");

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool continueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = _heroParser.GetContinueButton(doc);
                return continueButton is not null;
            };

            result = await chromeBrowser.Wait(continueShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}