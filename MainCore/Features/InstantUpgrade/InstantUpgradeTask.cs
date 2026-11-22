using MainCore.Features.Navigate;
using MainCore.Features.Update;
using MainCore.Features.UpgradeBuilding;

namespace MainCore.Features.InstantUpgrade;

[Handler]
public sealed partial class InstantUpgradeTask(
    ChromeManager chromeManager,
    ToDorfCommand.Handler toDorfCommand,
    InstantUpgradeCommand.Handler instantUpgradeCommand,
    UpdateBuildingCommand.Handler updateBuildingCommand,
    TaskManager taskManager)
{
    public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
    {
        protected override string TaskName => nameof(InstantUpgradeTask).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);
        Result result;

        result = await toDorfCommand.HandleAsync(new(browser, 0), cancellationToken);
        if (result.IsFailed) return result;

        var oldQueueCount = await browser.CurrentPage.CountQueueBuilding();
        if (oldQueueCount == 0) return Result.Ok();

        result = await instantUpgradeCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
        taskManager.Add(new UpgradeBuildingTask.Task(task.AccountId, task.VillageId), true);
        return Result.Ok();
    }
}