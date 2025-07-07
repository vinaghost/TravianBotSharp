namespace MainCore.Commands.Features.ClaimQuest
{
    [Handler]
    public static partial class ClaimQuestCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command _,
            IChromeBrowser browser,
            IDelayService delayService,
            SwitchTabCommand.Handler switchTabCommand,
            CancellationToken cancellationToken)
        {
            HtmlDocument html;
            Result result;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                html = browser.Html;
                var quest = QuestParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await switchTabCommand.HandleAsync(new(1), cancellationToken);
                    if (result.IsFailed) return result;

                    await delayService.DelayClick(cancellationToken);

                    quest = QuestParser.GetQuestCollectButton(browser.Html);
                    if (quest is null) return Result.Ok();

                    result = await browser.Click(By.XPath(quest.XPath));
                    if (result.IsFailed) return result;
                    continue;
                }

                result = await browser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result;
                await delayService.DelayClick(cancellationToken);
            }
            while (QuestParser.IsQuestClaimable(html));

            return Result.Ok();
        }
    }
}