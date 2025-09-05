#pragma warning disable S1172

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartAllFarmListCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IBrowser browser,
            CancellationToken cancellationToken
            )
        {
            var startAllButton = FarmListParser.GetStartAllButton(browser.Html);
            if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

            var result = await browser.Click(By.XPath(startAllButton.XPath));
            if (result.IsFailed) return result;

            // Farm list attack'lar başlatıldıktan sonra 7 saniye bekle
            // Bu süre tüm attack'ların gönderilmesi ve sayfanın stabilleşmesi için gerekli
            await Task.Delay(7000, cancellationToken);

            return Result.Ok();
        }
    }
}
