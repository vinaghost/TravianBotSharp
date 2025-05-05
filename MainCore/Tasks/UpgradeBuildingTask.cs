using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Errors.AutoBuilder;
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
            ITaskManager taskManager,
            IChromeManager chromeManager,
            ILogService logService,
            IDbContextFactory<AppDbContext> contextFactory,
            HandleJobCommand.Handler handleJobCommand,
            ToBuildPageCommand.Handler toBuildPageCommand,
            HandleResourceCommand.Handler handleResourceCommand,
            HandleUpgradeCommand.Handler handleUpgradeCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            var logger = logService.GetLogger(task.AccountId);
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                using var context = await contextFactory.CreateDbContextAsync();

                var (_, isFalied, plan, errors) = await handleJobCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFalied)
                {
                    if (errors.OfType<Continue>().Any()) continue;

                    var buildingQueue = context.GetFirstQueueBuilding(task.VillageId);
                    task.ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
                    return Skip.AutoBuilderBuildingQueueFull;
                }

                logger.Information("Build {Type} to level {Level} at location {Location}", plan.Type, plan.Level, plan.Location);

                result = await toBuildPageCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed) return result;

                result = await handleResourceCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<Continue>()) continue;
                    if (!result.HasError<WaitResource>(out var waitResourceError))
                    {
                        return result;
                    }

                    task.ExecuteAt = DateTime.Now.Add(waitResourceError.First().Time);
                    return Skip.AutoBuilderNotEnoughResource;
                }

                result = await handleUpgradeCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed) return result;
            }
        }

        private static QueueBuilding GetFirstQueueBuilding(this AppDbContext context, VillageId villageId)
        {
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return queueBuilding;
        }
    }
}