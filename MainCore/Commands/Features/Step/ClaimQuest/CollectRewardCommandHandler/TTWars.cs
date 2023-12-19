using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.ClaimQuest.CollectRewardCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TTWars)]
    public class TTWars : ICommandHandler<CollectRewardCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfCommand _unitOfCommand;

        public TTWars(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfCommand unitOfCommand)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(CollectRewardCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);

            HtmlDocument html;
            Result result;
            do
            {
                if (cancellationToken.IsCancellationRequested) return Result.Fail(new Cancel());
                html = chromeBrowser.Html;
                var quest = GetQuestLine(html);

                if (quest is null) return Result.Ok();

                result = await chromeBrowser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                bool collectShow(IWebDriver driver)
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(driver.PageSource);
                    var collect = _unitOfParser.QuestParser.GetQuestCollectButton(doc);
                    return collect is not null;
                };
                result = await chromeBrowser.Wait(collectShow, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                html = chromeBrowser.Html;
                var collect = _unitOfParser.QuestParser.GetQuestCollectButton(html);

                result = await chromeBrowser.Click(By.XPath(collect.XPath));
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await chromeBrowser.WaitPageLoaded(cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            while (_unitOfParser.QuestParser.IsQuestClaimable(html));
            return Result.Ok();
        }

        private static HtmlNode GetQuestLine(HtmlDocument doc)
        {
            var mentorTaskList = doc.GetElementbyId("mentorTaskList");
            if (mentorTaskList is null) return null;

            var button = mentorTaskList
                .Descendants("a")
                .Where(x => x.Descendants("svg").Any(x => x.HasClass("check")))
                .FirstOrDefault();
            return button;
        }
    }
}