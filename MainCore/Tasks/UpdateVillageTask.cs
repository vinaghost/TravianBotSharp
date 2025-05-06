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
            CancellationToken cancellationToken)
        {
            var url = browser.CurrentUrl;
            Result result;

            if (url.Contains("dorf1"))
            {
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
                result = await toDorfCommand.HandleAsync(new(task.AccountId, 1), cancellationToken);
                if (result.IsFailed) return result;

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }

            var seconds = settingService.ByName(task.VillageId, VillageSettingEnums.AutoRefreshMin, VillageSettingEnums.AutoRefreshMax, 60);
            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await taskManager.ReOrder(task.AccountId);
            return Result.Ok();
        }
    }
}