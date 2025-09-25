using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class UpgradeBuildingTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "Upgrade building";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ILogger logger,
            IChromeBrowser browser,
            GetBuildPlanCommand.Handler getBuildPlanCommand,
            ToBuildPageCommand.Handler toBuildPageCommand,
            HandleResourceCommand.Handler handleResourceCommand,
            AddCroplandCommand.Handler addCroplandCommand,
            HandleUpgradeCommand.Handler handleUpgradeCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            CancellationToken cancellationToken)
        {
            Result result;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var (_, isFailed, plan, errors) = await getBuildPlanCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed)
                {
                    var nextExecuteErrors = errors.OfType<NextExecuteError>().OrderBy(x => x.NextExecute).ToList();
                    if (nextExecuteErrors.Count > 0)
                    {
                        task.ExecuteAt = nextExecuteErrors.Select(x => x.NextExecute).Min();
                    }

                    return Skip.Error.WithErrors(errors);
                }

                logger.Information("Build {Type} to level {Level} at location {Location}", plan.Type, plan.Level, plan.Location);

                result = await toBuildPageCommand.HandleAsync(new(task.VillageId, plan), cancellationToken);
                if (result.IsFailed) return result;

                result = await handleResourceCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<LackOfFreeCrop>())
                    {
                        await addCroplandCommand.HandleAsync(new(task.VillageId), cancellationToken);
                        continue;
                    }

                    if (result.HasError<StorageLimit>())
                    {
                        return Stop.Error.WithErrors(result.Errors);
                    }
                    if (result.HasError<MissingResource>())
                    {
                        var time = UpgradeParser.GetTimeWhenEnoughResource(browser.Html, plan.Type);
                        task.ExecuteAt = DateTime.Now.Add(time);
                        return Skip.Error.WithErrors(result.Errors);
                    }

                    return result;
                }

                result = await handleUpgradeCommand.HandleAsync(new(task.VillageId, plan), cancellationToken);
                if (result.IsFailed) return result;

                logger.Information("Upgrade for {Type} at location {Location} completed successfully.", plan.Type, plan.Location);

                result = await updateBuildingCommand.HandleAsync(new(task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
        }
    }
}