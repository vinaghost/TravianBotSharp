using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class UpgradeBuildingTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName) : base(accountId, villageId, villageName)
            {
            }

            protected override string TaskName => "Upgrade building";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ILogger logger,
            IChromeBrowser browser,
            HandleJobCommand.Handler handleJobCommand,
            ToBuildPageCommand.Handler toBuildPageCommand,
            HandleResourceCommand.Handler handleResourceCommand,
            HandleUpgradeCommand.Handler handleUpgradeCommand,
            GetFirstQueueBuildingQuery.Handler getFirstQueueBuildingQuery,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            CancellationToken cancellationToken)
        {
            Result result;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var (_, isFailed, plan, errors) = await handleJobCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed)
                {
                    if (errors.OfType<Continue>().Any()) continue;

                    var buildingQueue = await getFirstQueueBuildingQuery.HandleAsync(new(task.VillageId), cancellationToken);
                    if (buildingQueue is null)
                    {
                        return Skip.BuildingJobQueueBroken;
                    }

                    task.ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
                    logger.Information("Construction queue is full. Schedule next run at {Time}", task.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    return Skip.ConstructionQueueFull;
                }

                logger.Information("Build {Type} to level {Level} at location {Location}", plan.Type, plan.Level, plan.Location);

                result = await toBuildPageCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed) return result;

                result = await handleResourceCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<Continue>()) continue;
                    if (result.HasError<Skip>())
                    {
                        var time = UpgradeParser.GetTimeWhenEnoughResource(browser.Html, plan.Type);
                        task.ExecuteAt = DateTime.Now.Add(time);
                        logger.Information("Not enough resource. Schedule next run at {Time}", task.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }

                    return result;
                }

                result = await handleUpgradeCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed) return result;

                logger.Information("Upgrade for {Type} at location {Location} completed successfully.", plan.Type, plan.Location);

                (_, isFailed, _, errors) = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed) return result;
            }
        }
    }
}