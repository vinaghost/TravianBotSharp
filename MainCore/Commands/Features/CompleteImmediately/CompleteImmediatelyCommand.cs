#pragma warning disable S1172

namespace MainCore.Commands.Features.CompleteImmediately
{
    [Handler]
    public static partial class CompleteImmediatelyCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IBrowser browser,
            CancellationToken cancellationToken)
        {
            var oldQueueCount = CompleteImmediatelyParser.CountQueueBuilding(browser.Html);

            if (oldQueueCount == 0) return Result.Ok();

            var completeNowButton = CompleteImmediatelyParser.GetCompleteButton(browser.Html);
            if (completeNowButton is null) return Retry.ButtonNotFound("complete now");

            var result = await browser.Click(By.XPath(completeNowButton.XPath));
            if (result.IsFailed) return result;

            static bool ConfirmShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var confirmButton = CompleteImmediatelyParser.GetConfirmButton(doc);
                return confirmButton is not null;
            }
            result = await browser.Wait(ConfirmShown, cancellationToken);
            if (result.IsFailed) return result;

            var confirmButton = CompleteImmediatelyParser.GetConfirmButton(browser.Html);
            if (confirmButton is null) return Retry.ButtonNotFound("confirm complete now");

            result = await browser.Click(By.XPath(confirmButton.XPath));
            if (result.IsFailed) return result;

            static bool QueueDifferent(IWebDriver driver, int oldQueueCount)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var newQueueCount = CompleteImmediatelyParser.CountQueueBuilding(doc);
                return oldQueueCount != newQueueCount;
            }

            result = await browser.Wait(driver => QueueDifferent(driver, oldQueueCount), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}
