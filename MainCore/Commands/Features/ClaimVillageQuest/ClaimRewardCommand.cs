using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.ClaimVillageQuest
{
    [Handler]
    public sealed partial class ClaimRewardCommand(SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(IChromeBrowser Browser) : ICommand
        {
            public string Name => nameof(ClaimRewardCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var browser = command.Browser;

            Result result;

            var quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Cancel.Error;
                }

                if (await quest.CountAsync() == 0)
                {
                    await Task.Delay(1000, CancellationToken.None);
                    if (await quest.CountAsync() == 0)
                    {
                        result = await switchTabCommand.HandleAsync(new(browser, 1), cancellationToken);
                        if (result.IsFailed) return result;
                    }

                    if (await quest.CountAsync() == 0)
                    {
                        return Result.Ok();
                    }
                }

                result = await browser.Click(quest.First);
                if (result.IsFailed) return result;
            }
        }
    }
}