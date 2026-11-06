using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Tasks
{
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
            var url = browser.CurrentUrl;
            Result result;

            if (url.Contains("dorf1"))
            {
                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;

                result = await toDorfCommand.HandleAsync(new(browser, 2), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
            else if (url.Contains("dorf2"))
            {
                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;

                result = await toDorfCommand.HandleAsync(new(browser, 1), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                result = await toDorfCommand.HandleAsync(new(browser, 2), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;

                result = await toDorfCommand.HandleAsync(new(browser, 1), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
            return Result.Ok();
        }
    }
}