#pragma warning disable S1172

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ToAdventurePageCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var result = await browser.Click(AdventureParser.GetHeroAdventureButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            result = await browser.WaitPageChanged("hero/adventures");
            if (result.IsFailed) return result;

            result = await browser.Wait(AdventureParser.GetAdventurePage(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}