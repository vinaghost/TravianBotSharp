using MainCore.Features.Navigate;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateBuildingTask(
    ChromeManager chromeManager,
    UpdateBuildingCommand.Handler updateBuildingCommand,
    ToDorfCommand.Handler toDorfCommand)
{
    public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
    {
        protected override string TaskName => nameof(UpdateBuildingTask).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);
        Result result;

        if (await browser.CurrentPage.GetDorf(1).CountAsync() > 0)
        {
            result = await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(browser, 2), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
        }
        else if (await browser.CurrentPage.GetDorf(2).CountAsync() > 0)
        {
            result = await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(browser, 1), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
        }
        else
        {
            result = await toDorfCommand.HandleAsync(new(browser, 2), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(browser, 1), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
        }
        return Result.Ok();
    }
}