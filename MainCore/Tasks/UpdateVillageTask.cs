using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class UpdateVillageTask(
        ChromeManager chromeManager,
        UpdateBuildingCommand.Handler updateBuildingCommand,
        ToDorfCommand.Handler toDorfCommand,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => nameof(UpdateVillageTask).Humanize();

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoRefreshEnable);
                if (!settingEnable) return false;

                return true;
            }
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

            task.ExecuteAt = GetNextExecuteTime(task.VillageId);
            return Result.Ok();
        }

        private DateTime GetNextExecuteTime(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var seconds = context.ByName(
                villageId,
                VillageSettingEnums.AutoRefreshMin,
                VillageSettingEnums.AutoRefreshMax,
                60);
            var nextExecute = DateTime.Now.AddSeconds(seconds);
            return nextExecute;
        }
    }
}