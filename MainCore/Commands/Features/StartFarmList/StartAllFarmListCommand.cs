#pragma warning disable S1172

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartAllFarmListCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken
            )
        {
            var startAllButton = FarmListParser.GetStartAllButton(browser.Html);
            if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

            var result = await browser.Click(By.XPath(startAllButton.XPath), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}