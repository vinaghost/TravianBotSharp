#pragma warning disable S1172

namespace MainCore.Commands.Features.CompleteImmediately
{
    [Handler]
    public static partial class CompleteImmediatelyCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var oldQueueCount = CompleteImmediatelyParser.CountQueueBuilding(browser.Html);

            if (oldQueueCount == 0) return Result.Ok();

            var (_, isFailed, element, errors) = await browser.GetElement(doc => CompleteImmediatelyParser.GetCompleteButton(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            (_, isFailed, element, errors) = await browser.GetElement(doc => CompleteImmediatelyParser.GetConfirmButton(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            result = await browser.Click(element, cancellationToken);
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