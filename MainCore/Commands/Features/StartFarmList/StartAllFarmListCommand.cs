using MainCore.Commands.Base;

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartAllFarmListCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var startAllButton = FarmListParser.GetStartAllButton(html);
            if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

            var result = await chromeBrowser.Click(By.XPath(startAllButton.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}