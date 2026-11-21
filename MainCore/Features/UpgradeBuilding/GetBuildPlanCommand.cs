using System.Text.Json;

using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.Features.UpgradeBuilding;

[Handler]
public sealed partial class GetBuildPlanCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    ILogger logger,
    RxQueue rxQueue)
{
    public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand
    {
        public string Name => nameof(GetBuildPlanCommand).Humanize();
    }

    private async ValueTask<Result<NormalBuildPlan>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var (accountId, villageId) = command;

        while (true)
        {
            if (cancellationToken.IsCancellationRequested) return Cancel.Error;

            using var context = contextFactory.CreateDbContext();
            var buildJobs = GetJobs(context, villageId);

            if (buildJobs.Count == 0)
            {
                logger.Warning("Job queue is empty.");
                return new UpgradeError(UpgradeErrorType.BuildingJobQueueEmpty);
            }

            var queueBuildings = GetQueueBuildings(context, villageId);

            JobDto job;
            if (queueBuildings.Count == 0)
            {
                job = buildJobs[0];
            }
            else
            {
                var (plusActive, applyRomanQueueLogic) = GetSetting(context, accountId, villageId);
                (_, var isFailed, job, var errors) = GetJob(buildJobs, queueBuildings, plusActive, applyRomanQueueLogic);
                if (isFailed) return Result.Fail(errors);
            }

            if (job.Type == JobTypeEnums.ResourceBuild)
            {
                var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);

                logger.Information("Extracting {ResourceBuildPlan}", resourceBuildPlan);

                var layoutBuildings = context.GetLayoutBuildings(villageId, ignoreJobBuilding: true);

                var normalBuildPlan = ExtractResourceBuildPlan(layoutBuildings, resourceBuildPlan);
                if (normalBuildPlan.Type == BuildingEnums.Unknown)
                {
                    context.DeleteJobById(job.Id);
                }
                else
                {
                    context.AddJob(villageId, normalBuildPlan, true);
                }

                rxQueue.Enqueue(new JobsModified(accountId, villageId));
                continue;
            }

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            if (IsPlanDone(context, villageId, plan))
            {
                context.DeleteJobById(job.Id);
                rxQueue.Enqueue(new JobsModified(accountId, villageId));
                continue;
            }
            return plan;
        }
    }

    private static List<JobDto> GetJobs(AppDbContext context, VillageId villageId)
    {
        return [.. context.Jobs
            .AsNoTracking()
            .Where(x => x.VillageId == villageId.Value)
            .Where(x => BuildJobTypes.Contains(x.Type))
            .OrderBy(x => x.Position)
            .Select(x => new JobDto
            {
                Id = new(x.Id),
                Position = x.Position,
                Type = x.Type,
                Content = x.Content,
            })];
    }

    private static List<QueueBuilding> GetQueueBuildings(AppDbContext context, VillageId villageId)
    {
        return [..  context.QueueBuildings
            .AsNoTracking()
            .Where(x => x.VillageId == villageId.Value)
            .OrderBy(x => x.CompleteTime)];
    }

    private static (bool PlusActive, bool ApplyRomanQueueLogic) GetSetting(AppDbContext context, AccountId accountId, VillageId villageId)
    {
        var plusActive = context.AccountsInfo
                            .Where(x => x.AccountId == accountId.Value)
                            .Select(x => x.HasPlusAccount)
                            .FirstOrDefault();

        var romanTribe = (TribeEnums)context.ByName(villageId, VillageSettingEnums.Tribe) == TribeEnums.Romans;

        var applyRomanQueueLogic = context.BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);
        return (plusActive, applyRomanQueueLogic && romanTribe);
    }

    private Result<JobDto> GetJob(List<JobDto> jobs, List<QueueBuilding> queueBuildings, bool plusActive, bool applyRomanQueueLogic)
    {
        if (queueBuildings.Count == 1)
        {
            if (plusActive)
            {
                return jobs[0];
            }

            if (applyRomanQueueLogic)
            {
                var job = GetJobBasedOnRomanLogic(queueBuildings, jobs);
                if (job is not null) return job;
            }

            logger.Warning("No job availabe. Wait for current building to finish at {CompleteTime}", queueBuildings[0].CompleteTime);
            return new UpgradeErrorNextExecute(UpgradeErrorType.ConstructionQueueFull, queueBuildings[0].CompleteTime);
        }

        if (queueBuildings.Count == 2)
        {
            if (plusActive && applyRomanQueueLogic)
            {
                var job = GetJobBasedOnRomanLogic(queueBuildings, jobs);
                if (job is not null) return job;
            }

            logger.Warning("No job availabe. Wait for current building to finish at {CompleteTime}", queueBuildings[0].CompleteTime);
            return new UpgradeErrorNextExecute(UpgradeErrorType.ConstructionQueueFull, queueBuildings[0].CompleteTime);
        }

        if (queueBuildings.Count == 3)
        {
            logger.Warning("No job availabe. Wait for current building to finish at {CompleteTime}", queueBuildings[0].CompleteTime);
            return new UpgradeErrorNextExecute(UpgradeErrorType.ConstructionQueueFull, queueBuildings[0].CompleteTime);
        }

        logger.Warning("No job availabe. But something strange happened. {QueueBuildingCount}", queueBuildings.Count);
        return new UpgradeError(UpgradeErrorType.BuildingJobQueueBroken);
    }

    private static readonly List<JobTypeEnums> BuildJobTypes = [
        JobTypeEnums.NormalBuild,
        JobTypeEnums.ResourceBuild
    ];

    public static readonly List<BuildingEnums> ResourceTypes = [
        BuildingEnums.Woodcutter,
        BuildingEnums.ClayPit,
        BuildingEnums.IronMine,
        BuildingEnums.Cropland,
    ];

    private static JobDto? GetJobBasedOnRomanLogic(List<QueueBuilding> queueBuildings, List<JobDto> jobs)
    {
        var countQueueBuilding = queueBuildings.Count;

        var countResourceQueueBuilding = queueBuildings.Count(x => ResourceTypes.Contains(x.Type));
        var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;

        var targetJob = countResourceQueueBuilding > countInfrastructureQueueBuilding
            ? GetInfrastructureBuildingJob(jobs)
            : GetResourceBuildingJob(jobs);

        return targetJob;
    }

    private static JobDto? GetInfrastructureBuildingJob(List<JobDto> jobs)
    {
        if (jobs.Count == 0) return null;

        return jobs
            .Where(x => x.Type == JobTypeEnums.NormalBuild)
            .OrderBy(x => x.Position)
            .FirstOrDefault(x =>
            {
                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content);
                return !ResourceTypes.Contains(plan.Type);
            });
    }

    private static JobDto? GetResourceBuildingJob(List<JobDto> jobs)
    {
        if (jobs.Count == 0) return null;

        var normalResourceJob = jobs
            .Where(x => x.Type == JobTypeEnums.NormalBuild)
            .OrderBy(x => x.Position)
            .FirstOrDefault(x =>
            {
                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content);
                return ResourceTypes.Contains(plan.Type);
            });

        var globalResourceJob = jobs
            .OrderBy(x => x.Position)
            .FirstOrDefault(x => x.Type == JobTypeEnums.ResourceBuild);

        if (normalResourceJob is null) return globalResourceJob;
        if (globalResourceJob is null) return normalResourceJob;

        return normalResourceJob.Position < globalResourceJob.Position
            ? normalResourceJob
            : globalResourceJob;
    }

    private static NormalBuildPlan ExtractResourceBuildPlan(List<BuildingItem> buildings, ResourceBuildPlan plan)
    {
        var candidateFields = plan.Plan switch
        {
            ResourcePlanEnums.ExcludeCrop => buildings.Where(x => x.Type == BuildingEnums.Woodcutter || x.Type == BuildingEnums.ClayPit || x.Type == BuildingEnums.IronMine),
            ResourcePlanEnums.OnlyCrop => buildings.Where(x => x.Type == BuildingEnums.Cropland),
            _ => buildings.Where(x => x.Type.IsResourceField())
        };

        var validFields = candidateFields.Where(x => x.Level < plan.Level).ToList();
        if (validFields.Count == 0) return new NormalBuildPlan(BuildingEnums.Unknown, 0, 0);

        var minLevel = validFields.Min(x => x.Level);
        var lowestFields = validFields.Where(x => x.Level == minLevel).ToList();

        var chosenOne = lowestFields[Random.Shared.Next(lowestFields.Count)];

        return new NormalBuildPlan()
        {
            Type = chosenOne.Type,
            Level = chosenOne.Level + 1,
            Location = chosenOne.Location,
        };
    }

    public static bool IsPlanDone(AppDbContext context, VillageId villageId, NormalBuildPlan plan)
    {
        var building = context.Buildings
            .AsNoTracking()
            .FirstOrDefault(x => x.VillageId == villageId.Value && x.Location == plan.Location);

        if (building is null || building.Type != plan.Type)
            return false;

        if (building.Level >= plan.Level)
            return true;

        var maxQueueLevel = context.QueueBuildings
            .AsNoTracking()
            .Where(x => x.VillageId == villageId.Value && x.Location == plan.Location)
            .OrderByDescending(x => x.Level)
            .Select(x => x.Level)
            .FirstOrDefault();

        return maxQueueLevel >= plan.Level;
    }
}