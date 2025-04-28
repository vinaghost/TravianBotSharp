using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.ClaimQuest
{
    [RegisterScoped<ClaimQuestCommand>]
    public class ClaimQuestCommand(IDataService dataService, StorageUpdated.Handler storageUpdate, SwitchTabCommand switchTabCommand, DelayClickCommand delayClickCommand) : CommandBase(dataService), ICommand
    {
        private readonly StorageUpdated.Handler _storageUpdate = storageUpdate;
        private readonly SwitchTabCommand _switchTabCommand = switchTabCommand;
        private readonly DelayClickCommand _delayClickCommand = delayClickCommand;

        public async Task<Result> Execute(CancellationToken cancellationToken)
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

                    await _storageUpdate.HandleAsync(new(accountId, villageId), cancellationToken);
                    return Result.Ok();
                }

                result = await chromeBrowser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                await _delayClickCommand.Execute(cancellationToken);
            }
            while (QuestParser.IsQuestClaimable(html));

            await _storageUpdate.HandleAsync(new(accountId, villageId), cancellationToken);

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

            result = await chromeBrowser.Click(By.XPath(quest.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}