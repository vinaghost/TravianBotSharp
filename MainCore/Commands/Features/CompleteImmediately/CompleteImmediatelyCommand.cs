using MainCore.Constraints;

namespace MainCore.Commands.Features.CompleteImmediately
{
    [Handler]
    public static partial class CompleteImmediatelyCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CompleteImmediatelyMessage.Handler completeImmediatelyMessage,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            var html = browser.Html;

            var completeNowButton = CompleteImmediatelyParser.GetCompleteButton(html);
            if (completeNowButton is null) return Retry.ButtonNotFound("complete now");

            static bool ConfirmShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var confirmButton = CompleteImmediatelyParser.GetConfirmButton(doc);
                return confirmButton is not null;
            }

            var result = await browser.Click(By.XPath(completeNowButton.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(ConfirmShown, cancellationToken);
            if (result.IsFailed) return result;

            html = browser.Html;
            var confirmButton = CompleteImmediatelyParser.GetConfirmButton(html);
            if (confirmButton is null) return Retry.ButtonNotFound("confirm complete now");

            var oldQueueCount = CompleteImmediatelyParser.CountQueueBuilding(html);

            static bool QueueDifferent(IWebDriver driver, int oldQueueCount)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var newQueueCount = CompleteImmediatelyParser.CountQueueBuilding(doc);
                return oldQueueCount != newQueueCount;
            }

            result = await browser.Click(By.XPath(confirmButton.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(driver => QueueDifferent(driver, oldQueueCount), cancellationToken);
            if (result.IsFailed) return result;

            await completeImmediatelyMessage.HandleAsync(new(accountId, villageId), cancellationToken);
            return Result.Ok();
        }
    }
}