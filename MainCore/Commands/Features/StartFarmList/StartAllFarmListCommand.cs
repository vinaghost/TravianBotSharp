#pragma warning disable S1172

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartAllFarmListCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser
            )
        {
            var result = await browser.Click(FarmListParser.GetStartAllButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}