using System.Text.Json;

using MainCore.Features.Navigate;
using MainCore.Features.Update;

namespace MainCore.Features.UpgradeBuilding;

[Handler]
public sealed partial class UpgradeBuildingTask(
    ILogger logger,
    RxQueue rxQueue,
    ChromeManager chromeManager,
    IDbContextFactory<AppDbContext> contextFactory,
    ToDorfCommand.Handler toDorfCommand,
    GetBuildPlanCommand.Handler getBuildPlanCommand,
    ToBuildPageCommand.Handler toBuildPageCommand,
    HandleResourceCommand.Handler handleResourceCommand,
    UpgradeCommand.Handler handleUpgradeCommand,
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

            result = await toDorfCommand.HandleAsync(new(browser, GetDorf(task.VillageId)), cancellationToken);
            if (result.IsFailed) return result;

            await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);

            var state = new BuildState();
            var (_, isFailed, errors) = await getBuildPlanCommand.HandleAsync(new(task.AccountId, task.VillageId, state), cancellationToken);
            if (isFailed)
            {
                var upgradeErrors = errors.OfType<UpgradeError>().ToList();
                if (upgradeErrors.Count == 0)
                    return Result.Fail(errors);

                var nextExecuteErrors = upgradeErrors.OfType<UpgradeErrorNextExecute>().ToList();
                if (nextExecuteErrors.Count > 0)
                {
                    var nextExecuteError = nextExecuteErrors.OrderBy(x => x.NextExecute).First();
                    return ReSchedule.Error(nextExecuteError.NextExecute);
                }

                if (upgradeErrors.Any(x => x.Type == UpgradeErrorType.BuildingJobQueueEmpty))
                {
                    return Skip.Error;
                }

                return Stop.Error;
            }

            var plan = state.Plan;
            logger.Information("Build {Plan}", plan);

            (_, isFailed, errors) = await toBuildPageCommand.HandleAsync(new(browser, plan), cancellationToken);
            if (isFailed)
            {
                var upgradeErrors = errors.OfType<UpgradeError>().ToList();
                if (upgradeErrors.Count == 0)
                    return Result.Fail(errors);

                if (upgradeErrors.Any(x => x.Type == UpgradeErrorType.BuildingComplete))
                {
                    // roll next plan
                    await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
                    continue;
                }

                if (upgradeErrors.Any(x => x.Type == UpgradeErrorType.BuildingNotExist))
                {
                    await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);

                    if (plan.Type.IsResourceBonus())
                    {
                        result = await toDorfCommand.HandleAsync(new(browser, 1), cancellationToken);
                        if (result.IsFailed) return result;

                        await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
                    }

                    (_, isFailed, errors) = CheckPrerequisite(task.VillageId, plan);
                    if (isFailed)
                    {
                        var nextExecuteErrors = upgradeErrors.OfType<UpgradeErrorNextExecute>().ToList();
                        if (nextExecuteErrors.Count > 0)
                        {
                            var nextExecuteError = nextExecuteErrors.OrderBy(x => x.NextExecute).First();
                            return ReSchedule.Error(nextExecuteError.NextExecute);
                        }

                        return Stop.Error;
                    }

                    // continue
                    result = await toBuildPageCommand.HandleAsync(new(browser, plan, true), cancellationToken);
                    if (result.IsFailed) return result;
                }
                else if (upgradeErrors.Any(x => x.Type == UpgradeErrorType.BuildingInQueue))
                {
                    await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);

                    if (IsPlanUnderConstruction(task.VillageId, plan))
                    {
                        // roll next plan
                        continue;
                    }

                    if (HasUnlocatedQueueEntry(task.VillageId, plan))
                    {
                        logger.Warning("Cannot determine whether {Type} at #{Location} is already queued; its queue location is unknown.", plan.Type, plan.Location);
                        return Retry.Error;
                    }

                    // continue
                    result = await toBuildPageCommand.HandleAsync(new(browser, plan, true), cancellationToken);
                    if (result.IsFailed) return result;
                }
            }
            var canUseHeroResource = CanUseHeroResource(task.VillageId);
            (_, isFailed, errors) = await handleResourceCommand.HandleAsync(new(browser, plan, canUseHeroResource), cancellationToken);
            if (isFailed)
            {
                var upgradeErrors = errors.OfType<UpgradeError>().ToList();
                if (upgradeErrors.Count == 0)
                    return Result.Fail(errors);

                if (upgradeErrors.Any(x => x.Type == UpgradeErrorType.LackOfFreeCrop))
                {
                    AddCropland(task.AccountId, task.VillageId);
                    continue;
                }

                if (upgradeErrors.Any(x => x.Type == UpgradeErrorType.StorageLimit))
                {
                    return Stop.Error;
                }
                if (upgradeErrors.Any(x => x.Type == UpgradeErrorType.MissingResource))
                {
                    var time = await browser.CurrentPage.GetTimeWhenEnoughResource(plan.Type);
                    return ReSchedule.Error(DateTime.Now.Add(time));
                }
            }

            LogBuildingInfo(task.VillageId, plan);

            var canUseSpecialUpgrade = CanUseSpecialUpgrade(task.VillageId);

            result = await handleUpgradeCommand.HandleAsync(new(browser, plan, canUseSpecialUpgrade), cancellationToken);
            if (result.IsFailed) return result;

            logger.Information("Upgrade for {Type} at #{Location} completed successfully.", plan.Type, plan.Location);

            await updateBuildingCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
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

    private bool HasUnlocatedQueueEntry(VillageId villageId, NormalBuildPlan plan)
    {
        using var context = contextFactory.CreateDbContext();
        return context.QueueBuildings.Any(x =>
            x.VillageId == villageId.Value
            && x.Location == -1
            && x.Type == plan.Type
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
                    errors.Add(new UpgradeErrorNextExecute(UpgradeErrorType.PrerequisiteInQueue, queueBuilding.CompleteTime));
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
            errors.Add(new UpgradeErrorNextExecute(UpgradeErrorType.PrerequisiteInQueue, highestLevelInQueue.CompleteTime));
        }
        else
        {
            logger.Information("{Type} need one building at max level before build more, current one is {Level} at #{Location}.", highestExistingBuilding.Type, highestExistingBuilding.Level, highestExistingBuilding.Location);
            errors.Add(new UpgradeError(UpgradeErrorType.PrerequisiteMissing));
        }

        return Result.FailIfNotEmpty(errors);
    }

    private bool CanUseHeroResource(VillageId villageId)
    {
        using var context = contextFactory.CreateDbContext();
        return context.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);
    }

    private bool CanUseSpecialUpgrade(VillageId villageId)
    {
        using var context = contextFactory.CreateDbContext();
        var useSpecialUpgrade = context.BooleanByName(villageId, VillageSettingEnums.UseSpecialUpgrade);
        return useSpecialUpgrade;
    }

    private void AddCropland(AccountId accountId, VillageId villageId)
    {
        using var context = contextFactory.CreateDbContext();
        var buildings = context.GetLayoutBuildings(villageId, true);

        var cropland = buildings
            .Where(x => x.Type == BuildingEnums.Cropland)
            .OrderBy(x => x.Level)
            .First();

        var cropLandPlan = new NormalBuildPlan()
        {
            Location = cropland.Location,
            Type = cropland.Type,
            Level = cropland.Level + 1,
        };

        context.AddJob(villageId, cropLandPlan, true);
        rxQueue.Enqueue(new JobsModified(accountId, villageId));
    }

    private void LogBuildingInfo(VillageId villageId, NormalBuildPlan plan)
    {
        using var context = contextFactory.CreateDbContext();

        var queueBuilding = context.QueueBuildings
            .Where(x => x.VillageId == villageId.Value)
            .FirstOrDefault(x => x.Location == plan.Location);

        if (queueBuilding is not null)
        {
            logger.Information("{Type} at location {Location} is in queue at level {Level}", queueBuilding.Type, queueBuilding.Location, queueBuilding.Level);
        }
        else
        {
            var building = context.Buildings
               .Where(x => x.VillageId == villageId.Value)
               .FirstOrDefault(x => x.Location == plan.Location);

            if (building is not null)
            {
                logger.Information("{Type} at location {Location} is at level {Level}", building.Type, building.Location, building.Level);
            }
        }
    }

    private int GetDorf(VillageId villageId)
    {
        using var context = contextFactory.CreateDbContext();
        var jobs = context.Jobs
            .AsNoTracking()
            .Where(x => x.VillageId == villageId.Value)
            .Where(x => GetBuildPlanCommand.BuildJobTypes.Contains(x.Type))
            .ToList();

        if (jobs.Count == 0) return 0;

        var job = jobs[0];
        if (job.Type == JobTypeEnums.ResourceBuild) return 1;
        if (job.Type == JobTypeEnums.NormalBuild)
        {
            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
            if (plan.Type.IsResourceField()) return 1;
            return 2;
        }
        return 0;
    }
}