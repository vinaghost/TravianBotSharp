using HtmlAgilityPack;

namespace MainCore.Commands.Features.Step.ClaimQuest.CollectRewardCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TravianOfficial)]
    public class TravianOfficial : ICommandHandler<CollectRewardCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfCommand _unitOfCommand;

        public TravianOfficial(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfCommand unitOfCommand)
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
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
                html = chromeBrowser.Html;
                var quest = _questParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await Handle(chromeBrowser);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                    result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    continue;
                }

                result = await chromeBrowser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            while (_questParser.IsQuestClaimable(html));
            return Result.Ok();
        }

        private async Task<Result> Handle(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var firstTab = _navigationTabParser.GetTab(html, 0);
            if (firstTab is null) return Retry.NotFound("tasks", "tab");

            var firstTabActive = _navigationTabParser.IsTabActive(firstTab);

            Result result;
            if (firstTabActive)
            {
                var secondTab = _navigationTabParser.GetTab(html, 1);
                result = await chromeBrowser.Click(By.XPath(secondTab.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                result = await chromeBrowser.Click(By.XPath(firstTab.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }
    }
}