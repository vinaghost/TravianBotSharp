using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class CompleteImmediatelyTask : VillageTask
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public CompleteImmediatelyTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IChromeManager chromeManager, UnitOfParser unitOfParser) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 0), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await CompleteImmediately();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            await _mediator.Publish(new CompleteImmediatelyMessage(AccountId, VillageId), CancellationToken);
            return Result.Ok();
        }

        protected override void SetName()
        {
            var villageName = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Complete immediately in {villageName}";
        }

        public async Task<Result> CompleteImmediately()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);

            var html = chromeBrowser.Html;

            var completeNowButton = _unitOfParser.CompleteImmediatelyParser.GetCompleteButton(html);
            if (completeNowButton is null) return Result.Fail(Retry.ButtonNotFound("complete now"));

            Result result;

            result = await chromeBrowser.Click(By.XPath(completeNowButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool confirmShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var confirmButton = _unitOfParser.CompleteImmediatelyParser.GetConfirmButton(doc);
                return confirmButton is not null;
            };

            result = await chromeBrowser.Wait(confirmShown, CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var confirmButton = _unitOfParser.CompleteImmediatelyParser.GetConfirmButton(html);
            if (confirmButton is null) return Result.Fail(Retry.ButtonNotFound("complete now"));

            var oldQueueCount = _unitOfParser.QueueBuildingParser.Get(html)
                .Where(x => x.Level != -1)
                .Count();

            result = await chromeBrowser.Click(By.XPath(confirmButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool queueDifferent(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var newQueueCount = _unitOfParser.QueueBuildingParser.Get(doc)
                    .Where(x => x.Level != -1)
                    .Count();
                return oldQueueCount != newQueueCount;
            };
            result = await chromeBrowser.Wait(queueDifferent, CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}