using MainCore.Features.Navigate;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateFarmListTask(
    ChromeManager chromeManager,
    ToFarmListPageCommand.Handler toFarmListPageCommand,
    UpdateFarmlistCommand.Handler updateFarmlistCommand)
{
    public sealed class Task(AccountId accountId) : AccountTask(accountId)
    {
        protected override string TaskName => nameof(UpdateFarmListTask).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);
        Result result;
        result = await toFarmListPageCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;
        result = await updateFarmlistCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}