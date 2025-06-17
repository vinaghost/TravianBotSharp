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
            var countJob = context.Jobs
               .Where(x => x.VillageId == villageId.Value)
               .Where(x => BuildJobTypes.Contains(x.Type))
               .Count();

            if (countJob == 0) return Skip.BuildingJobQueueEmpty;

            var countQueueBuilding = context.CountQueueBuilding(villageId);

            if (countQueueBuilding == 0)
            {
                var result = context.GetBuildingJob(villageId, false);
                return result;
            }

            var plusActive = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.HasPlusAccount)
                .FirstOrDefault();

            var applyRomanQueueLogic = context.BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (countQueueBuilding == 1)
            {
                if (plusActive)
                {
                    var result = context.GetBuildingJob(villageId, false);
                    return result;
                }

                if (applyRomanQueueLogic)
                {
                    var result = context.GetBuildingJob(villageId, true);
                    return result;
                }

                return JobError.ConstructionQueueFull;
            }

            if (countQueueBuilding == 2)
            {
                if (plusActive && applyRomanQueueLogic)
                {
                    var result = context.GetBuildingJob(villageId, true);
                    return result;
                }
                return JobError.ConstructionQueueFull;
            }

            return JobError.ConstructionQueueFull;
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

        private static Result<JobDto> GetBuildingJob(this AppDbContext context, VillageId villageId, bool romanLogic)
        {
            JobDto job;
            if (romanLogic)
            {
                var romanResult = context.GetJobBasedOnRomanLogic(villageId);
                if (romanResult.IsFailed) return Result.Fail(romanResult.Errors);
                job = romanResult.Value;
            }
            else
            {
                job = context.GetBuildingJob(villageId);
            }

            if (job.Type == JobTypeEnums.ResourceBuild) return job;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;
            if (plan.Type.IsResourceField()) return job;
            var (isSuccess, _, erros) = context.IsJobValid(villageId, plan);
            if (isSuccess) return job;
            return Result.Fail(erros);
        }

        private static JobDto GetBuildingJob(
            this AppDbContext context,
            VillageId villageId)
        {
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => BuildJobTypes.Contains(x.Type))
                .OrderBy(x => x.Position)
                .ToDto()
                .First();
            return job;
        }

        private static Result<JobDto> GetJobBasedOnRomanLogic(
            this AppDbContext context,
            VillageId villageId)
        {
            var countQueueBuilding = context.CountQueueBuilding(villageId);
            var countResourceQueueBuilding = context.CountResourceQueueBuilding(villageId);
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;
            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                var job = context.GetInfrastructureBuildingJob(villageId);
                if (job is null) return JobError.JobNotAvailable("Infrastructure building");
                return job;
            }
            else
            {
                var job = context.GetResourceBuildingJob(villageId);
                if (job is null) return JobError.JobNotAvailable("Resource field");
                return job;
            }
        }

        private static int CountQueueBuilding(
            this AppDbContext context,
            VillageId villageId)
        {
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Level != -1)
                .Count();
            return count;
        }

        private static int CountResourceQueueBuilding(
            this AppDbContext context,
            VillageId villageId)
        {
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Level != -1)
                .Where(x => ResourceTypes.Contains(x.Type))
                .Count();
            return count;
        }

        private static JobDto GetInfrastructureBuildingJob(
            this AppDbContext context,
            VillageId villageId)
        {
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)!
                })
                .Where(x => !ResourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .First();
            return job;
        }

        private static JobDto? GetResourceBuildingJob(
            this AppDbContext context,
            VillageId villageId)
        {
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)!
                })
                .Where(x => ResourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();

            var resourceBuildJob = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.ResourceBuild)
                .ToDto()
                .FirstOrDefault();

            if (job is null) return resourceBuildJob;
            if (resourceBuildJob is null) return job;
            if (job.Position < resourceBuildJob.Position) return job;
            return resourceBuildJob;
        }

        private static Result IsJobValid(
            this AppDbContext context,
            VillageId villageId,
            NormalBuildPlan plan)
        {
            var buildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            var oldBuilding = buildings
                .Where(x => x.Location == plan.Location)
                .FirstOrDefault();

            if (oldBuilding is not null && oldBuilding.Type == plan.Type) return Result.Ok();

            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            var errors = new List<JobError>();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = buildings
                    .Any(x => x.Type == prerequisiteBuilding.Type && x.Level >= prerequisiteBuilding.Level);
                if (!vaild) errors.Add(JobError.PrerequisiteBuildingMissing(prerequisiteBuilding.Type, prerequisiteBuilding.Level));
            }

            if (!plan.Type.IsMultipleBuilding()) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);

            var firstBuilding = buildings
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (firstBuilding is null) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
            if (firstBuilding.Location == plan.Location) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
            if (firstBuilding.Level == firstBuilding.Type.GetMaxLevel()) return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);

            errors.Add(JobError.PrerequisiteBuildingMissing(firstBuilding.Type, firstBuilding.Level));
            return Result.Fail(errors);
        }
    }
}