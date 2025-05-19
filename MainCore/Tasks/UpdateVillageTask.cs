using MainCore.Commands.NextExecute;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class UpdateVillageTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName) : base(accountId, villageId, villageName)
            {
            }

            protected override string TaskName => "Update village";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ITaskManager taskManager,
            IChromeBrowser browser,
            ISettingService settingService,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToDorfCommand.Handler toDorfCommand,
            NextExecuteUpdateVillageTaskCommand.Handler nextExecuteUpdateVillageTaskCommand,
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
            }
            else if (url.Contains("dorf2"))
            {
                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);
                ;
                result = await toDorfCommand.HandleAsync(new(task.AccountId, 1), cancellationToken);
                if (result.IsFailed) return result;

                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);
            }
            else
            {
                result = await toDorfCommand.HandleAsync(new(task.AccountId, 1), cancellationToken);
                if (result.IsFailed) return result;

                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);
            }

            await nextExecuteUpdateVillageTaskCommand.HandleAsync(task, cancellationToken);
            return Result.Ok();
        }
    }
}