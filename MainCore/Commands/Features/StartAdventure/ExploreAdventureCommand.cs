#pragma warning disable S1172

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ExploreAdventureCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (!(await AdventureParser.CanStartAdventure(browser.CurrentPage))) return Skip.Error.WithError("No adventure available");

            var adventures = await AdventureParser.GetAdventureInfo(browser.CurrentPage);
            if (adventures.Count == 0) return Skip.Error.WithError("No adventure available");

            var adventure = adventures[0];

            logger.Information("Start {Difficult} adventure takes {Duration} ", adventure.Difficult, adventure.Duration);

            var result = await browser.Click(adventure.Button);
            if (result.IsFailed) return result;

            result = await browser.Wait(AdventureParser.GetContinueButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}