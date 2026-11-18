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

        var attempt = 0;
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

            switch (attempt)
            {
                case 0:
                    result = await switchTabCommand.HandleAsync(new(browser, 1), cancellationToken);
                    if (result.IsFailed) return result;
                    break;

                case 1:
                    result = await switchTabCommand.HandleAsync(new(browser, 0), cancellationToken);
                    if (result.IsFailed) return result;
                    break;

                case 2:
                    result = await browser.Refresh();
                    if (result.IsFailed) return result;
                    break;

                default:
                    return Result.Ok();
            }

            attempt++;
        }
    }
}