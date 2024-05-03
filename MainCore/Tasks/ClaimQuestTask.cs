using MainCore.Commands.Features.ClaimQuest;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class ClaimQuestTask : VillageTask
    {
        private readonly IQuestParser _questParser;
        private readonly DelayClickCommand _delayClickCommand;

        public ClaimQuestTask(IChromeManager chromeManager, IMediator mediator, IVillageRepository villageRepository, IQuestParser questParser, DelayClickCommand delayClickCommand) : base(chromeManager, mediator, villageRepository)
        {
            _questParser = questParser;
            _delayClickCommand = delayClickCommand;
        }

        protected override async Task<Result> Execute()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            Result result;

            result = await _mediator.Send(new ToQuestPageCommand(chromeBrowser), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await CollectReward(chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _mediator.Publish(new StorageUpdated(AccountId, VillageId), CancellationToken);
            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _villageRepository.GetVillageName(VillageId);
            _name = $"Claim quest in {village}";
        }

        private async Task<Result> CollectReward(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            HtmlDocument html;
            Result result;
            do
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
                html = chromeBrowser.Html;
                var quest = _questParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await ClaimAccountQuest(chromeBrowser);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    return Result.Ok();
                }

                result = await chromeBrowser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                await _delayClickCommand.Execute(AccountId);
            }
            while (_questParser.IsQuestClaimable(html));
            return Result.Ok();
        }

        private async Task<Result> ClaimAccountQuest(IChromeBrowser chromeBrowser)
        {
            Result result;
            result = await _mediator.Send(new SwitchTabCommand(chromeBrowser, 1));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await _delayClickCommand.Execute(AccountId);

            var quest = _questParser.GetQuestCollectButton(chromeBrowser.Html);

            if (quest is null) return Result.Ok();

            result = await chromeBrowser.Click(By.XPath(quest.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}