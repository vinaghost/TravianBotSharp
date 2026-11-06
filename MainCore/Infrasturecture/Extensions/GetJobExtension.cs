using System.Collections.Frozen;
using System.Text.Json;

namespace MainCore.Infrasturecture.Extensions
{
    public static class GetJobExtension
    {
        public static Result<JobDto> GetJob(this AppDbContext context, AccountId accountId, VillageId villageId)
        {
            var buildJobs = context.Jobs
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => BuildJobTypes.Contains(x.Type))
                .OrderBy(x => x.Position)
                .ToDto()
                .ToList();

            if (buildJobs.Count == 0)
            {
                return new UpgradeError(UpgradeErrorType.BuildingJobQueueEmpty);
            }

            var queueBuildings = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.CompleteTime)
                .ToList();

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
                    var job = GetJobBasedOnRomanLogic(queueBuildings, buildJobs);
                    if (job is null) return new UpgradeError(UpgradeErrorType.ConstructionQueueFull)
                    {
                        NextExecute = queueBuildings[0].CompleteTime,
                    };
                    return job;
                }

                return new UpgradeError(UpgradeErrorType.ConstructionQueueFull)
                {
                    NextExecute = queueBuildings[0].CompleteTime,
                };
            }

            if (queueBuildings.Count == 2)
            {
                if (plusActive && applyRomanQueueLogic)
                {
                    var job = GetJobBasedOnRomanLogic(queueBuildings, buildJobs);
                    if (job is null) return new UpgradeError(UpgradeErrorType.ConstructionQueueFull)
                    {
                        NextExecute = queueBuildings[0].CompleteTime,
                    };
                    return job;
                }

                return new UpgradeError(UpgradeErrorType.ConstructionQueueFull)
                {
                    NextExecute = queueBuildings[0].CompleteTime,
                };
            }

            if (queueBuildings.Count == 3)
            {
                return new UpgradeError(UpgradeErrorType.ConstructionQueueFull)
                {
                    NextExecute = queueBuildings[0].CompleteTime,
                };
            }

            return new UpgradeError(UpgradeErrorType.BuildingJobQueueBroken);
        }

        private static readonly FrozenSet<JobTypeEnums> BuildJobTypes = [
            JobTypeEnums.NormalBuild,
            JobTypeEnums.ResourceBuild
        ];

        public static readonly FrozenSet<BuildingEnums> ResourceTypes = [
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
                    return plan != null && !ResourceTypes.Contains(plan.Type);
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
                    return plan != null && ResourceTypes.Contains(plan.Type);
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
    }
}