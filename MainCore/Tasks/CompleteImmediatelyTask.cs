using HtmlAgilityPack;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class CompleteImmediatelyTask : VillageTask
    {
        private readonly ICompleteImmediatelyParser _completeImmediatelyParser;
        private readonly IQueueBuildingParser _queueBuildingParser;

        public CompleteImmediatelyTask(IChromeManager chromeManager, IMediator mediator, IVillageRepository villageRepository, ICompleteImmediatelyParser completeImmediatelyParser, IQueueBuildingParser queueBuildingParser) : base(chromeManager, mediator, villageRepository)
        {
            _completeImmediatelyParser = completeImmediatelyParser;
            _queueBuildingParser = queueBuildingParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await _mediator.Send(ToDorfCommand.ToDorf(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await CompleteImmediately();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _mediator.Publish(new CompleteImmediatelyMessage(AccountId, VillageId), CancellationToken);
            return Result.Ok();
        }

        protected override void SetName()
        {
            var villageName = _villageRepository.GetVillageName(VillageId);
            _name = $"Complete immediately in {villageName}";
        }

        public async Task<Result> CompleteImmediately()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);

            var html = chromeBrowser.Html;

            var completeNowButton = _completeImmediatelyParser.GetCompleteButton(html);
            if (completeNowButton is null) return Retry.ButtonNotFound("complete now");

            Result result;

            result = await chromeBrowser.Click(By.XPath(completeNowButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool confirmShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var confirmButton = _completeImmediatelyParser.GetConfirmButton(doc);
                return confirmButton is not null;
            };

            result = await chromeBrowser.Wait(confirmShown, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var confirmButton = _completeImmediatelyParser.GetConfirmButton(html);
            if (confirmButton is null) return Retry.ButtonNotFound("complete now");

            var oldQueueCount = _queueBuildingParser.Get(html)
                .Where(x => x.Level != -1)
                .Count();

            result = await chromeBrowser.Click(By.XPath(confirmButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool queueDifferent(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var newQueueCount = _queueBuildingParser.Get(doc)
                    .Where(x => x.Level != -1)
                    .Count();
                return oldQueueCount != newQueueCount;
            };
            result = await chromeBrowser.Wait(queueDifferent, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}