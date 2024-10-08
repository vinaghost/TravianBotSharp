using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient<UpgradeBuildingTask>]
    public class UpgradeBuildingTask(IDbContextFactory<AppDbContext> contextFactory) : VillageTask
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;

            HandleJobCommand handleJobCommand = null;
            ToBuildPageCommand toBuildPageCommand = null;
            HandleResourceCommand handleResourceCommand = null;
            HandleUpgradeCommand handleUpgradeCommand = null;

            var dataService = scoped.ServiceProvider.GetRequiredService<DataService>();
            var logger = dataService.Logger;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                handleJobCommand ??= scoped.ServiceProvider.GetRequiredService<HandleJobCommand>();
                var (_, isFalied, plan, errors) = await handleJobCommand.Execute(cancellationToken);
                if (isFalied)
                {
                    if (errors.OfType<Continue>().Any()) continue;
                    if (!errors.OfType<BuildingQueueFull>().Any())
                    {
                        return Result.Fail(errors).WithError(TraceMessage.Error(TraceMessage.Line()));
                    }

                    SetTimeQueueBuildingComplete();
                    return Skip.AutoBuilderBuildingQueueFull;
                }

                logger.Information("Build {Type} to level {Level} at location {Location}", plan.Type, plan.Level, plan.Location);

                toBuildPageCommand ??= scoped.ServiceProvider.GetRequiredService<ToBuildPageCommand>();
                result = await toBuildPageCommand.Execute(plan, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                handleResourceCommand ??= scoped.ServiceProvider.GetRequiredService<HandleResourceCommand>();
                result = await handleResourceCommand.Execute(plan, cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<Continue>()) continue;
                    if (!result.HasError<WaitResource>(out var waitResourceError))
                    {
                        return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }

                    ExecuteAt = DateTime.Now.Add(waitResourceError.First().Time);
                    return Skip.AutoBuilderNotEnoughResource;
                }

                handleUpgradeCommand ??= scoped.ServiceProvider.GetRequiredService<HandleUpgradeCommand>();
                result = await handleUpgradeCommand.Execute(plan, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
        }

        protected override string TaskName => "Upgrade building";

        private void SetTimeQueueBuildingComplete()
        {
            var buildingQueue = GetFirstQueueBuilding(VillageId);
            if (buildingQueue is null)
            {
                return;
            }

            ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
        }

        private QueueBuilding GetFirstQueueBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return queueBuilding;
        }
    }
}