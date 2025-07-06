using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class UpdateBuildingTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "Update building";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            IChromeBrowser browser,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToDorfCommand.Handler toDorfCommand,
            CancellationToken cancellationToken)
        {
            var url = browser.CurrentUrl;
            Result result;

            bool isFailed;
            List<IError> errors;
            if (url.Contains("dorf1"))
            {
                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);

                result = await toDorfCommand.HandleAsync(new(task.AccountId, 2), cancellationToken);
                if (result.IsFailed) return result;

                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);
            }
            else if (url.Contains("dorf2"))
            {
                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);

                result = await toDorfCommand.HandleAsync(new(task.AccountId, 1), cancellationToken);
                if (result.IsFailed) return result;

                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);
            }
            else
            {
                result = await toDorfCommand.HandleAsync(new(task.AccountId, 2), cancellationToken);
                if (result.IsFailed) return result;

                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);

                result = await toDorfCommand.HandleAsync(new(task.AccountId, 1), cancellationToken);
                if (result.IsFailed) return result;

                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);
            }
            return Result.Ok();
        }
    }
}