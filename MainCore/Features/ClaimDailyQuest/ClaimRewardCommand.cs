namespace MainCore.Features.ClaimDailyQuest;

[Handler]
public static partial class ClaimRewardCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(ClaimRewardCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;

        var collectButtons = browser.CurrentPage.GetDailyQuestCollectButton();
        var result = await browser.Click(collectButtons.First);
        if (result.IsFailed) return result;

        var claimButton = browser.CurrentPage.GetDailyQuestClaimButton();
        result = await browser.Click(claimButton);
        if (result.IsFailed) return result;

        await browser.Refresh();
        return Result.Ok();
    }
}