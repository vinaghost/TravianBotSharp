using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
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
                result = await handleJobCommand.Execute(cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<Continue>()) continue;
                    if (!result.HasError<BuildingQueueFull>())
                    {
                        return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                    SetTimeQueueBuildingComplete();
                    return Skip.AutoBuilderBuildingQueueFull;
                }

                var plan = handleJobCommand.Data;

                logger.Information("Build {Type} to level {Level} at location {Location}", plan.Type, plan.Level, plan.Location);

                toBuildPageCommand ??= scoped.ServiceProvider.GetRequiredService<ToBuildPageCommand>();
                result = await toBuildPageCommand.Execute(plan, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                handleResourceCommand ??= scoped.ServiceProvider.GetRequiredService<HandleResourceCommand>();
                result = await handleResourceCommand.Execute(cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<Continue>()) continue;
                    if (!result.HasError<WaitResource>())
                    {
                        return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }

                    var waitResourceError = result.Errors.OfType<WaitResource>().First();
                    ExecuteAt = DateTime.Now.Add(waitResourceError.Time);
                    return Skip.AutoBuilderNotEnoughResource;
                }

                handleUpgradeCommand = scoped.ServiceProvider.GetRequiredService<HandleUpgradeCommand>();
                result = await handleUpgradeCommand.Execute(plan, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                return Result.Ok();
            }
        }

        protected override void SetName()
        {
            var village = new GetVillageName().Execute(VillageId);
            _name = $"Upgrade building in {village}";
        }

        private void SetTimeQueueBuildingComplete()
        {
            var buildingQueue = GetFirstQueueBuilding(VillageId);
            if (buildingQueue is null)
            {
                ExecuteAt = DateTime.Now.AddSeconds(1);
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