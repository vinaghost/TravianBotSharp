using MainCore.Constraints;
using System.Text.Json;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetJobQuery
    {
        public sealed record Query(AccountId AccountId, VillageId VillageId) : IAccountVillageQuery;

        private static async ValueTask<Result<JobDto>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;

            var (accountId, villageId) = query;

            var buildJobs = context.GetBuildJobs(villageId);
            if (buildJobs.Count == 0) return UpgradeBuildingError.BuildingJobQueueEmpty;

            var queueBuildings = context.GetQueueBuildings(villageId);

            if (queueBuildings.Count == 0)
            {
                var job = buildJobs.First();
                var result = context.IsJobValid(villageId, job);
                if (result.IsFailed) return result;
                return job;
            }

            var plusActive = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.HasPlusAccount)
                .FirstOrDefault();

            var applyRomanQueueLogic = context.BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (queueBuildings.Count == 1)
            {
                // each
                if (!plusActive && !applyRomanQueueLogic)
                {
                    return NextExecuteError.ConstructionQueueFull(queueBuildings[0].CompleteTime.AddSeconds(3));
                }

                var (_, isFailed, job, errors) = GetJobBasedOnRomanLogic(queueBuildings, buildJobs);
                if (isFailed) return Result.Fail(errors);
                var result = context.IsJobValid(villageId, job);
                if (result.IsFailed) return result;
                return job;
            }

            if (queueBuildings.Count == 2)
            {
                // both
                if (!plusActive || !applyRomanQueueLogic)
                {
                    return NextExecuteError.ConstructionQueueFull(queueBuildings[0].CompleteTime.AddSeconds(3));
                }

                var (_, isFailed, job, errors) = GetJobBasedOnRomanLogic(queueBuildings, buildJobs);
                if (isFailed) return Result.Fail(errors);
                var result = context.IsJobValid(villageId, job);
                if (result.IsFailed) return result;
                return job;
            }

            return UpgradeBuildingError.BuildingJobQueueBroken;
        }

        private static List<QueueBuilding> GetQueueBuildings(this AppDbContext context, VillageId villageId)
        {
            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Level != -1)
                .OrderBy(x => x.CompleteTime)
                .ToList();
            return queueBuildings;
        }

        private static List<JobDto> GetBuildJobs(this AppDbContext context, VillageId villageId)
        {
            var jobs = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => BuildJobTypes.Contains(x.Type))
                .OrderBy(x => x.Position)
                .ToDto()
                .ToList();
            return jobs;
        }

        private static List<JobTypeEnums> BuildJobTypes = [
            JobTypeEnums.NormalBuild,
            JobTypeEnums.ResourceBuild
        ];

        public static readonly List<BuildingEnums> ResourceTypes = [
            BuildingEnums.Woodcutter,
            BuildingEnums.ClayPit,
            BuildingEnums.IronMine,
            BuildingEnums.Cropland,
        ];

        private static Result<JobDto> GetJobBasedOnRomanLogic(List<QueueBuilding> queueBuildings, List<JobDto> jobs)
        {
            var countQueueBuilding = queueBuildings.Count;
            var countResourceQueueBuilding = CountResourceQueueBuilding(queueBuildings);
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;

            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                var job = GetInfrastructureBuildingJob(jobs);
                if (job is null) return UpgradeBuildingError.JobNotAvailable("Infrastructure building");
                return job;
            }
            else
            {
                var job = GetResourceBuildingJob(jobs);
                if (job is null) return UpgradeBuildingError.JobNotAvailable("Resource field");
                return job;
            }
        }

        private static int CountResourceQueueBuilding(List<QueueBuilding> queueBuildings)
        {
            var count = queueBuildings
                .Where(x => ResourceTypes.Contains(x.Type))
                .Count();
            return count;
        }

        private static JobDto? GetInfrastructureBuildingJob(List<JobDto> jobs)
        {
            var job = jobs
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)!
                })
                .Where(x => !ResourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return job;
        }

        private static JobDto? GetResourceBuildingJob(List<JobDto> jobs)
        {
            var job = jobs
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)!
                })
                .Where(x => ResourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();

            var resourceBuildJob = jobs
                .Where(x => x.Type == JobTypeEnums.ResourceBuild)
                .FirstOrDefault();

            if (job is null) return resourceBuildJob;
            if (resourceBuildJob is null) return job;
            if (job.Position < resourceBuildJob.Position) return job;
            return resourceBuildJob;
        }

        private static Result IsJobValid(
            this AppDbContext context,
            VillageId villageId,
            JobDto job)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return Result.Ok();

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;
            if (plan.Type.IsResourceField()) return Result.Ok();

            var buildings = context.Buildings
               .Where(x => x.VillageId == villageId.Value)
               .ToList();

            var oldBuilding = buildings
               .Where(x => x.Location == plan.Location)
               .FirstOrDefault();

            if (oldBuilding is not null && oldBuilding.Type == plan.Type) return Result.Ok();

            var errors = new List<UpgradeBuildingError>();
            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = buildings
                   .Any(x => x.Type == prerequisiteBuilding.Type && x.Level >= prerequisiteBuilding.Level);

                if (!vaild) errors.Add(UpgradeBuildingError.PrerequisiteBuildingMissing(prerequisiteBuilding.Type, prerequisiteBuilding.Level));
            }

            if (!plan.Type.IsMultipleBuilding()) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);

            var firstBuilding = buildings
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (firstBuilding is null) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
            if (firstBuilding.Location == plan.Location) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
            if (firstBuilding.Level == firstBuilding.Type.GetMaxLevel()) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);

            errors.Add(UpgradeBuildingError.PrerequisiteBuildingMissing(firstBuilding.Type, firstBuilding.Level));
            return Result.Fail(errors);
        }
    }
}