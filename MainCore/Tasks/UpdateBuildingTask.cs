using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class UpdateBuildingTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName, DateTime executeAt) : base(accountId, villageId, villageName, executeAt)
            {
            }

            protected override string TaskName => "Update building";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            IChromeManager chromeManager,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToDorfCommand.Handler toDorfCommand,
            CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(task.AccountId);
            var url = browser.CurrentUrl;
            Result result;
            if (url.Contains("dorf1"))
            {
                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;

                result = await toDorfCommand.HandleAsync(new(task.AccountId, 2), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
            else if (url.Contains("dorf2"))
            {
                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;

                result = await toDorfCommand.HandleAsync(new(task.AccountId, 1), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                result = await toDorfCommand.HandleAsync(new(task.AccountId, 2), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;

                result = await toDorfCommand.HandleAsync(new(task.AccountId, 1), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
            return Result.Ok();
        }
    }
}