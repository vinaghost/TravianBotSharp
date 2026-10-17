namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public sealed partial class ExploreAdeventureCommand(ILogger logger)
    {
        public sealed record Command(IChromeBrowser Browser);

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
            Result result;

            var adventures = await AdventureParser.GetAdventureInfo(browser.CurrentPage);
            if (adventures.Count == 0) return Skip.Error.WithError("No adventure available");

            var adventure = adventures[0];

            logger.Information("Start {Difficult} adventure takes {Duration} ", adventure.Difficult, adventure.Duration);

            result = await browser.Click(adventure.Button);
            if (result.IsFailed) return result;

            result = await browser.Wait(AdventureParser.GetContinueButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}