using MainCore.Features.Navigate;

namespace MainCore.Features.ClaimVillageQuest;

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

        var quest = browser.CurrentPage.GetVillageQuestCollectButton();

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Cancel.Error;
            }

            if (await quest.CountAsync() != 0)
            {
                result = await browser.Click(quest.First);
                if (result.IsFailed) return result;
                continue;
            }

            await Task.Delay(1000, CancellationToken.None);

            if (await quest.CountAsync() != 0)
            {
                continue;
            }

            result = await switchTabCommand.HandleAsync(new(browser, 1), cancellationToken);
            if (result.IsFailed) return result;

            if (await quest.CountAsync() != 0)
            {
                continue;
            }

            return Result.Ok();
        }
    }
}