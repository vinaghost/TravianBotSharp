using HtmlAgilityPack;

namespace MainCore.Commands.Features.ClaimQuest
{
    public class CollectRewardCommand : ByAccountIdBase, ICommand
    {
        public IChromeBrowser ChromeBrowser { get; }

        public CollectRewardCommand(AccountId accountId, IChromeBrowser chromeBrowser) : base(accountId)
        {
            ChromeBrowser = chromeBrowser;
        }
    }

    public class CollectRewardCommandHandler : ICommandHandler<CollectRewardCommand>
    {
        private readonly IQuestParser _questParser;
        private readonly DelayClickCommand _delayClickCommand;
        private readonly INavigationTabParser _navigationTabParser;

        public CollectRewardCommandHandler(IQuestParser questParser, DelayClickCommand delayClickCommand, INavigationTabParser navigationTabParser)
        {
            _questParser = questParser;
            _delayClickCommand = delayClickCommand;
            _navigationTabParser = navigationTabParser;
        }

        public async Task<Result> Handle(CollectRewardCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = request.ChromeBrowser;

            HtmlDocument html;
            Result result;
            do
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
                html = chromeBrowser.Html;
                var quest = _questParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    var resultSwitchTab = await Handle(chromeBrowser);
                    if (resultSwitchTab.IsFailed) return Result.Fail(resultSwitchTab.Errors).WithError(TraceMessage.Error(TraceMessage.Line()));

                    if (!resultSwitchTab.Value) return Result.Ok();

                    await _delayClickCommand.Execute(accountId);
                    continue;
                }

                result = await chromeBrowser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await _delayClickCommand.Execute(accountId);
            }
            while (_questParser.IsQuestClaimable(html));
            return Result.Ok();
        }

        private async Task<Result<bool>> Handle(IChromeBrowser chromeBrowser)
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

                return true;
            }
            return false;
        }
    }
}