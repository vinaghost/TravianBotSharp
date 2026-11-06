using System.Numerics;

using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class UpgradeBuildingTask(
        ILogger logger,
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory,
        ToDorfCommand.Handler toDorfCommand,
        GetBuildPlanCommand.Handler getBuildPlanCommand,
        ToBuildPageCommand.Handler toBuildPageCommand,
        HandleResourceCommand.Handler handleResourceCommand,
        HandleUpgradeCommand.Handler handleUpgradeCommand,
        UpdateBuildingCommand.Handler updateBuildingCommand)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => nameof(UpgradeBuildingTask).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            Result result;
            var browser = chromeManager.Get(task.AccountId);
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var (_, isFailed, plan, errors) = await getBuildPlanCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (isFailed)
                {
                    var inQueueError = errors.OfType<UpgradeError>()
                        .Where(x => x.Type == UpgradeErrorType.PrerequisiteInQueue || x.Type == UpgradeErrorType.BuildingInQueue)
                        .OrderBy(x => x.NextExecute)
                        .FirstOrDefault();

                    if (inQueueError is not null)
                    {
                        task.ExecuteAt = inQueueError.NextExecute!.Value;
                        return Skip.Error;
                    }
                    return Result.Fail(errors);
                }

                logger.Information("Build {Plan}", plan);

                result = await toBuildPageCommand.HandleAsync(new(browser, plan), cancellationToken);
                if (result.IsFailed)
                {
                    if (!result.HasError<UpgradeError>())
                        return result;

                    var upgradeError = result.Errors.OfType<UpgradeError>().First();
                    if (upgradeError.Type == UpgradeErrorType.BuildingComplete)
                    {
                        // roll next plan
                        continue;
                    }
                    else if (upgradeError.Type == UpgradeErrorType.BuildingInQueue)
                    {
                        result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                        if (result.IsFailed) return result;

                        if (IsPlanUnderConstruction(task.VillageId, plan))
                        {
                            // roll next plan
                            continue;
                        }

                        // continue
                        result = await toBuildPageCommand.HandleAsync(new(browser, plan, true), cancellationToken);
                        if (result.IsFailed) return result;
                    }
                    else if (upgradeError.Type == UpgradeErrorType.BuildingNotExist)
                    {
                        result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                        if (result.IsFailed) return result;

                        if (plan.Type.IsResourceBonus())
                        {
                            result = await toDorfCommand.HandleAsync(new(browser, 1), cancellationToken);
                            if (result.IsFailed) return result;

                            result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                            if (result.IsFailed) return result;
                        }

                        result = CheckPrerequisite(task.VillageId, plan);
                        if (result.IsFailed)
                        {
                            if (!result.HasError<UpgradeError>())
                                return result;

                            var prerequisiteInQueueError = result.Errors.OfType<UpgradeError>()
                                .Where(x => x.Type == UpgradeErrorType.PrerequisiteInQueue)
                                .OrderBy(x => x.NextExecute)
                                .FirstOrDefault();

                            if (prerequisiteInQueueError is not null)
                            {
                                task.ExecuteAt = prerequisiteInQueueError.NextExecute!.Value;
                                return Skip.Error;
                            }

                            return Stop.Error;
                        }

                        // continue
                        result = await toBuildPageCommand.HandleAsync(new(browser, plan, true), cancellationToken);
                        if (result.IsFailed) return result;
                    }
                }

                result = await handleResourceCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<LackOfFreeCrop>())
                        continue;

                    if (result.HasError<StorageLimit>())
                    {
                        return Stop.Error.WithErrors(result.Errors);
                    }
                    if (result.HasError<MissingResource>())
                    {
                        var time = await UpgradeParser.GetTimeWhenEnoughResource(browser.CurrentPage, plan.Type);
                        task.ExecuteAt = DateTime.Now.Add(time);
                        return Skip.Error.WithErrors(result.Errors);
                    }

                    return result;
                }

                result = await handleUpgradeCommand.HandleAsync(new(task.AccountId, task.VillageId, plan), cancellationToken);
                if (result.IsFailed) return result;

                logger.Information("Upgrade for {Type} at #{Location} completed successfully.", plan.Type, plan.Location);

                result = await updateBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
                if (result.IsFailed) return result;
            }
        }

        private bool IsPlanUnderConstruction(VillageId villageId, NormalBuildPlan plan)
        {
            using var context = contextFactory.CreateDbContext();
            return context.QueueBuildings.Any(x =>
                x.VillageId == villageId.Value
                && x.Location == plan.Location
                && x.Level >= plan.Level);
        }

        private Result CheckPrerequisite(VillageId villageId, NormalBuildPlan plan)
        {
            using var context = contextFactory.CreateDbContext();
            var buildings = context.Buildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .GroupBy(x => x.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    MaxLevel = g.Max(y => y.Level)
                })
                .ToDictionary(x => x.Type, x => x.MaxLevel);

            var queueBuildings = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            var errors = new List<IError>();
            var prerequisiteBuildings = plan.Type.GetPrerequisites();

            foreach (var prereq in prerequisiteBuildings)
            {
                var buildingLevel = buildings.GetValueOrDefault(prereq.Type, 0);

                if (buildingLevel < prereq.Level)
                {
                    var queueBuilding = queueBuildings
                        .Where(x => x.Type == prereq.Type)
                        .MinBy(x => x.CompleteTime);

                    if (queueBuilding is not null)
                    {
                        logger.Information("Prerequisite {Type} level {Level} is in queue, will complete at {CompleteTime}.", prereq.Type, prereq.Level, queueBuilding.CompleteTime);
                        errors.Add(new UpgradeError(UpgradeErrorType.PrerequisiteInQueue)
                        {
                            NextExecute = queueBuilding.CompleteTime
                        });
                    }
                    else
                    {
                        errors.Add(new UpgradeError(UpgradeErrorType.PrerequisiteMissing));
                    }
                }
            }

            if (!plan.Type.IsMultipleBuilding()) return Result.FailIfNotEmpty(errors);

            var highestExistingBuilding = context.Buildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (highestExistingBuilding is null)
                return Result.FailIfNotEmpty(errors);

            if (highestExistingBuilding.Location == plan.Location)
                return Result.FailIfNotEmpty(errors);

            if (highestExistingBuilding.Level == highestExistingBuilding.Type.GetMaxLevel())
                return Result.FailIfNotEmpty(errors);

            var highestLevelInQueue = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == highestExistingBuilding.Location)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (highestLevelInQueue is not null && highestLevelInQueue.Level > highestExistingBuilding.Level)
            {
                logger.Information("{Type} need one building at max level before build more, current one at #{Location} is in progress to {Level}, will complete at {CompleteTime}.", highestExistingBuilding.Type, highestExistingBuilding.Location, highestExistingBuilding.Level, highestLevelInQueue.CompleteTime);
                errors.Add(new UpgradeError(UpgradeErrorType.PrerequisiteInQueue)
                {
                    NextExecute = highestLevelInQueue.CompleteTime
                });
            }
            else
            {
                logger.Information("{Type} need one building at max level before build more, current one is {Level} at #{Location}.", highestExistingBuilding.Type, highestExistingBuilding.Level, highestExistingBuilding.Location);
                errors.Add(new UpgradeError(UpgradeErrorType.PrerequisiteMissing));
            }

            return Result.FailIfNotEmpty(errors);
        }
    }
}