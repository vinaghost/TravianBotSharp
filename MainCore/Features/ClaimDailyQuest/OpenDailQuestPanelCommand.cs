namespace MainCore.Features.ClaimDailyQuest;

[Handler]
public static partial class OpenDailQuestPanelCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(OpenDailQuestPanelCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;
        Result result;
        var dailyQuestButton = browser.CurrentPage.GetDailyQuestButton();
        result = await browser.Click(dailyQuestButton);
        if (result.IsFailed) return result;

        var getDailyQuestItems = browser.CurrentPage.GetDailyQuestItems();
        result = await browser.Wait(getDailyQuestItems);
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}