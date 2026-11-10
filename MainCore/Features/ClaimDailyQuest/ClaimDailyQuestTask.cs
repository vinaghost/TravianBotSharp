namespace MainCore.Features.ClaimDailyQuest;

[Handler]
public sealed partial class ClaimDailyQuestTask(
    ChromeManager chromeManager,
    OpenDailQuestPanelCommand.Handler openDailQuestPanelCommand,
    ClaimRewardCommand.Handler claimRewardCommand)
{
    public sealed class Task(AccountId accountId) : AccountTask(accountId)
    {
        protected override string TaskName => nameof(ClaimDailyQuestTask).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);
        var canClaimQuest = await browser.CurrentPage.IsDailyQuestClaimable();
        if (!canClaimQuest) return Result.Ok();

        var result = await openDailQuestPanelCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        result = await claimRewardCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}