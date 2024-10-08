using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.CompleteImmediately
{
    [RegisterScoped<CompleteImmediatelyCommand>]
    public class CompleteImmediatelyCommand(DataService dataService, IMediator mediator) : CommandBase(dataService), ICommand
    {
        private readonly IMediator _mediator = mediator;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var completeNowButton = CompleteImmediatelyParser.GetCompleteButton(html);
            if (completeNowButton is null) return Retry.ButtonNotFound("complete now");

            bool confirmShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var confirmButton = CompleteImmediatelyParser.GetConfirmButton(doc);
                return confirmButton is not null;
            }

            Result result;

            result = await chromeBrowser.Click(By.XPath(completeNowButton.XPath), confirmShown, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var confirmButton = CompleteImmediatelyParser.GetConfirmButton(html);
            if (confirmButton is null) return Retry.ButtonNotFound("complete now");

            var oldQueueCount = CompleteImmediatelyParser.CountQueueBuilding(html);

            bool queueDifferent(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var newQueueCount = CompleteImmediatelyParser.CountQueueBuilding(doc);
                return oldQueueCount != newQueueCount;
            }

            result = await chromeBrowser.Click(By.XPath(confirmButton.XPath), queueDifferent, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _mediator.Publish(new CompleteImmediatelyMessage(_dataService.AccountId, _dataService.VillageId), cancellationToken);

            return Result.Ok();
        }
    }
}