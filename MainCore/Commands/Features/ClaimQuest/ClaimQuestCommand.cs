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
                var quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);

                if (await quest.CountAsync() == 0)
                {
                    result = await switchTabCommand.HandleAsync(new(1), cancellationToken);
                    if (result.IsFailed) return result;

                    await delayService.DelayClick(cancellationToken);

                    quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);
                    result = await browser.Click(quest);
                    if (result.IsFailed) return result;
                    continue;
                }
                else
                {
                    result = await browser.Click(quest);
                    if (result.IsFailed) return result;
                    await delayService.DelayClick(cancellationToken);
                }
            }
            while (await QuestParser.IsQuestClaimable(browser.CurrentPage));

            return Result.Ok();
        }
    }
}