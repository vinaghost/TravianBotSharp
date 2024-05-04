using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.ClaimQuest
{
    public class ClaimQuestCommand : QuestCommand
    {
        private readonly DelayClickCommand _delayClickCommand;

        public ClaimQuestCommand(DelayClickCommand delayClickCommand)
        {
            _delayClickCommand = delayClickCommand;
        }

        public async Task<Result> Execute(AccountId accountId, IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            HtmlDocument html;
            Result result;
            do
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
                html = chromeBrowser.Html;
                var quest = GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await ClaimAccountQuest(accountId, chromeBrowser, cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    return Result.Ok();
                }

                result = await chromeBrowser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                await _delayClickCommand.Execute(accountId);
            }
            while (IsQuestClaimable(html));
            return Result.Ok();
        }

        private async Task<Result> ClaimAccountQuest(AccountId accountId, IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            Result result;
            result = await new SwitchTabCommand().Execute(chromeBrowser, 1, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await _delayClickCommand.Execute(accountId);

            var quest = GetQuestCollectButton(chromeBrowser.Html);

            if (quest is null) return Result.Ok();

            result = await chromeBrowser.Click(By.XPath(quest.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetQuestCollectButton(HtmlDocument doc)
        {
            var taskTable = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("taskOverview"))
                .FirstOrDefault();
            if (taskTable is null) return null;

            var button = taskTable
                .Descendants("button")
                .Where(x => x.HasClass("collect") && !x.HasClass("disabled"))
                .FirstOrDefault();
            return button;
        }
    }
}