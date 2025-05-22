using MainCore.Constraints;

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartAllFarmListCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {

            var html = browser.Html;

            var startAllButton = FarmListParser.GetStartAllButton(html);
            if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

            var result = await browser.Click(By.XPath(startAllButton.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}