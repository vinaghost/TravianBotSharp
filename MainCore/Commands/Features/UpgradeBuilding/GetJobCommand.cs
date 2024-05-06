using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class GetJobCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GetJobCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public Result<JobDto> Execute(AccountId accountId, VillageId villageId)
        {
            var countJob = CountBuildingJob(villageId);
            if (countJob == 0) return Skip.AutoBuilderJobQueueEmpty;

            var countQueueBuilding = CountQueueBuilding(villageId);
            if (countQueueBuilding == 0) return GetBuildingJob(villageId);

            var plusActive = new IsPlusActive().Execute(accountId);
            var applyRomanQueueLogic = new GetVillageSetting().BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (countQueueBuilding == 1)
            {
                if (plusActive) return GetBuildingJob(villageId);
                if (applyRomanQueueLogic) return GetBuildingJob(villageId, true);
                return BuildingQueue.Full;
            }

            if (countQueueBuilding == 2)
            {
                if (plusActive && applyRomanQueueLogic) return GetBuildingJob(villageId, true);
                return BuildingQueue.Full;
            }

            return BuildingQueue.Full;
        }

        private JobDto GetBuildingJob(VillageId villageId, bool romanLogic = false)
        {
            var job = romanLogic ? GetJobBasedOnRomanLogic(villageId) : GetBuildingJob(villageId);
            return job;
        }

        private JobDto GetJobBasedOnRomanLogic(VillageId villageId)
        {
            var countQueueBuilding = CountQueueBuilding(villageId);
            var countResourceQueueBuilding = CountResourceQueueBuilding(villageId);
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;
            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                return GetInfrastructureBuildingJob(villageId);
            }
            else
            {
                return GetResourceBuildingJob(villageId);
            }
        }

        private int CountQueueBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }

        private int CountResourceQueueBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var resourceTypes = new List<BuildingEnums>()
            {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland
            };

            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => resourceTypes.Contains(x.Type))
                .Count();
            return count;
        }

        private JobDto GetInfrastructureBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var resourceTypes = new List<BuildingEnums>()
            {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland
            };

            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)
                })
                .Where(x => !resourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return job;
        }

        private JobDto GetResourceBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var resourceTypes = new List<BuildingEnums>()
            {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland
            };
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)
                })
                .Where(x => resourceTypes.Contains(x.Content.Type))
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

        private int CountBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var types = new List<JobTypeEnums>()
            {
                JobTypeEnums.NormalBuild,
                JobTypeEnums.ResourceBuild
            };
            var count = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => types.Contains(x.Type))
                .Count();
            return count;
        }
    }
}