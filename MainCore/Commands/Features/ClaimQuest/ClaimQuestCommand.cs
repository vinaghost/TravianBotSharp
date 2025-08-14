#pragma warning disable S1172

namespace MainCore.Commands.Features.ClaimQuest
{
    [Handler]
    public static partial class ClaimQuestCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            IDelayService delayService,
            SwitchTabCommand.Handler switchTabCommand,
            CancellationToken cancellationToken)
        {
            Result result;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Cancel.Error;
                }
                var quest = QuestParser.GetQuestCollectButton(browser.Html);

                if (quest is null)
                {
                    result = await switchTabCommand.HandleAsync(new(1), cancellationToken);
                    if (result.IsFailed) return result;

                    await delayService.DelayClick(cancellationToken);

                    quest = QuestParser.GetQuestCollectButton(browser.Html);
                    if (quest is null) return Result.Ok();

                    result = await browser.Click(By.XPath(quest.XPath), cancellationToken);
                    if (result.IsFailed) return result;
                    continue;
                }

                result = await browser.Click(By.XPath(quest.XPath), cancellationToken);
                if (result.IsFailed) return result;
                await delayService.DelayClick(cancellationToken);
            }
            while (QuestParser.IsQuestClaimable(browser.Html));

            return Result.Ok();
        }
    }
}