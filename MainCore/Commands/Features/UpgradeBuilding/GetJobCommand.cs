using System.Text.Json;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class GetJobCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result<JobDto>> HandleAsync(
            Command command,
            AppDbContext context
            )
        {
            await Task.CompletedTask;

            var (accountId, villageId) = command;

            var buildJobs = context.GetBuildJobs(villageId);
            if (buildJobs.Count == 0) return UpgradeBuildingError.BuildingJobQueueEmpty;

            var (buildings, queueBuildings) = context.GetBuildings(villageId);

            if (queueBuildings.Count == 0)
            {
                return buildJobs[0];
            }

            var plusActive = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.HasPlusAccount)
                .FirstOrDefault();

            var applyRomanQueueLogic = context.BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (queueBuildings.Count == 1)
            {
                if (plusActive)
                {
                    return buildJobs[0];
                }

                if (applyRomanQueueLogic)
                {
                    var (_, isFailed, job, errors) = GetJobBasedOnRomanLogic(queueBuildings, buildJobs);
                    if (isFailed) return Result.Fail(errors);
                    return job;
                }
                return NextExecuteError.ConstructionQueueFull(queueBuildings[0].CompleteTime);
            }

            if (queueBuildings.Count == 2)
            {
                if (plusActive && applyRomanQueueLogic)
                {
                    var (_, isFailed, job, errors) = GetJobBasedOnRomanLogic(queueBuildings, buildJobs);
                    if (isFailed) return Result.Fail(errors);
                    return job;
                }
                return NextExecuteError.ConstructionQueueFull(queueBuildings[0].CompleteTime);
            }

            if (queueBuildings.Count == 3)
            {
                return NextExecuteError.ConstructionQueueFull(queueBuildings[0].CompleteTime);
            }

            return UpgradeBuildingError.BuildingJobQueueBroken;
        }

        private static (List<Building>, List<QueueBuilding>) GetBuildings(this AppDbContext context, VillageId villageId)
        {
            var completeQueueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.CompleteTime < DateTime.Now)
                .OrderBy(x => x.Level)
                .ToList();

            if (completeQueueBuildings.Count > 0)
            {
                foreach (var completeQueueBuilding in completeQueueBuildings)
                {
                    if (completeQueueBuilding.Location == -1) continue;

                    var building = context.Buildings
                        .Where(x => x.VillageId == villageId.Value)
                        .Where(x => x.Location == completeQueueBuilding.Location)
                        .FirstOrDefault();
                    if (building is null) continue;

                    building.Level = completeQueueBuilding.Level;
                    context.Remove(completeQueueBuilding);
                }
                context.SaveChanges();
            }

            var buildings = context.Buildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            var queueBuildings = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.CompleteTime)
                .ToList();

            return (buildings, queueBuildings);
        }

        private static List<JobDto> GetBuildJobs(this AppDbContext context, VillageId villageId)
        {
            var jobs = context.Jobs
                .AsNoTracking()
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

            var job = countResourceQueueBuilding > countInfrastructureQueueBuilding ? GetInfrastructureBuildingJob(jobs) : GetResourceBuildingJob(jobs);
            if (job is null) return NextExecuteError.ConstructionQueueFull(queueBuildings[0].CompleteTime);
            return job;
        }

        private static int CountResourceQueueBuilding(List<QueueBuilding> queueBuildings)
        {
            var count = queueBuildings
                .Count(x => ResourceTypes.Contains(x.Type));
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
                .FirstOrDefault(x => x.Type == JobTypeEnums.ResourceBuild);

            if (job is null) return resourceBuildJob;
            if (resourceBuildJob is null) return job;
            if (job.Position < resourceBuildJob.Position) return job;
            return resourceBuildJob;
        }
    }
}