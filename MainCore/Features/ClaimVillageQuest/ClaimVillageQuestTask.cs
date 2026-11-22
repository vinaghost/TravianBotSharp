using MainCore.Features.UpgradeBuilding;

namespace MainCore.Features.ClaimVillageQuest;

[Handler]
public sealed partial class ClaimVillageQuestTask(
    ChromeManager chromeManager,
    OpenVillageQuestPageCommand.Handler openVillageQuestPageCommand,
    ClaimRewardCommand.Handler claimRewardCommand,
    TaskManager taskManager)
{
    public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
    {
        protected override string TaskName => nameof(ClaimVillageQuestTask).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);

        var canClaimQuest = await browser.CurrentPage.GetNewQuestSpeechBubble().CountAsync() > 0;
        if (!canClaimQuest) return Result.Ok();

        var result = await openVillageQuestPageCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        result = await claimRewardCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        taskManager.Add(new UpgradeBuildingTask.Task(task.AccountId, task.VillageId), true);
        return Result.Ok();
    }
}