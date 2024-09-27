using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.ClaimQuest
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ClaimQuestCommand(DataService dataService, IMediator mediator, SwitchTabCommand switchTabCommand, DelayClickCommand delayClickCommand) : CommandBase(dataService)
    {
        private readonly IMediator _mediator = mediator;
        private readonly SwitchTabCommand _switchTabCommand = switchTabCommand;
        private readonly DelayClickCommand _delayClickCommand = delayClickCommand;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var accountId = _dataService.AccountId;
            var villageId = _dataService.VillageId;
            HtmlDocument html;
            Result result;
            do
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
                html = chromeBrowser.Html;
                var quest = QuestParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await ClaimAccountQuest(cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                    await _mediator.Publish(new StorageUpdated(accountId, villageId), cancellationToken);
                    return Result.Ok();
                }

                result = await chromeBrowser.Click(By.XPath(quest.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                await _delayClickCommand.Execute(cancellationToken);
            }
            while (QuestParser.IsQuestClaimable(html));

            await _mediator.Publish(new StorageUpdated(accountId, villageId), cancellationToken);

            return Result.Ok();
        }

        private async Task<Result> ClaimAccountQuest(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;

            Result result;
            result = await _switchTabCommand.Execute(1, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await _delayClickCommand.Execute(cancellationToken);

            var quest = QuestParser.GetQuestCollectButton(chromeBrowser.Html);

            if (quest is null) return Result.Ok();

            result = await chromeBrowser.Click(By.XPath(quest.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}