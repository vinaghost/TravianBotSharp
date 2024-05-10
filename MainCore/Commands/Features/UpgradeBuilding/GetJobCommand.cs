using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class GetJobCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogService _logService;

        public GetJobCommand(IDbContextFactory<AppDbContext> contextFactory = null, ILogService logService = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
            _logService = logService ?? Locator.Current.GetService<ILogService>();
        }

        public async Task<Result<JobDto>> Execute(IChromeBrowser chromeBrowser, AccountId accountId, VillageId villageId, CancellationToken cancellationToken)
        {
            var countJob = CountBuildingJob(villageId);
            if (countJob == 0) return Skip.AutoBuilderJobQueueEmpty;

            var countQueueBuilding = CountQueueBuilding(villageId);
            if (countQueueBuilding == 0) return await GetBuildingJob(chromeBrowser, accountId, villageId, false, cancellationToken);

            var plusActive = new IsPlusActive().Execute(accountId);
            var applyRomanQueueLogic = new GetSetting().BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (countQueueBuilding == 1)
            {
                if (plusActive) return await GetBuildingJob(chromeBrowser, accountId, villageId, false, cancellationToken);
                if (applyRomanQueueLogic) return await GetBuildingJob(chromeBrowser, accountId, villageId, true, cancellationToken);
                return BuildingQueue.Full;
            }

            if (countQueueBuilding == 2)
            {
                if (plusActive && applyRomanQueueLogic) return await GetBuildingJob(chromeBrowser, accountId, villageId, true, cancellationToken);
                return BuildingQueue.Full;
            }

            return BuildingQueue.Full;
        }

        private async Task<Result<JobDto>> GetBuildingJob(IChromeBrowser chromeBrowser, AccountId accountId, VillageId villageId, bool romanLogic, CancellationToken cancellationToken)
        {
            var job = romanLogic ? GetJobBasedOnRomanLogic(villageId) : GetBuildingJob(villageId);
            if (job is null) return BuildingQueue.Full;
            if (job.Type == JobTypeEnums.ResourceBuild) return job;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            if (plan.Type.IsResourceField()) return job;

            var valid = IsJobValid(villageId, plan);

            if (valid.IsFailed)
            {
                Result result;
                result = await new ToDorfCommand().Execute(chromeBrowser, 2, true, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await new UpdateBuildingCommand().Execute(chromeBrowser, accountId, villageId, cancellationToken);

                valid = IsJobValid(villageId, plan);

                if (valid.IsFailed)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();

                    var logger = _logService.GetLogger(accountId);

                    bool isProgressing = false;
                    logger.Warning("Try to build {Type} level {Level} but", plan.Type, plan.Level);
                    foreach (var error in valid.Errors)
                    {
                        if (error is PrerequisiteBuildingMissing bqError)
                        {
#pragma warning disable S6966 // Awaitable method should be used
                            if (!isProgressing && context.QueueBuildings
                                .Where(x => x.VillageId == villageId.Value)
                                .Where(x => x.Type == bqError.PrerequisiteBuilding)
                                .Any(x => x.Level >= bqError.Level))
                            {
                                isProgressing = true;
                            }
#pragma warning restore S6966 // Awaitable method should be used
                            logger.Warning("{Message}", bqError.Message);
                        }
                    }
                    if (isProgressing) return BuildingQueue.Full;

                    logger.Information("Removed task while waiting user fix auto build queue order");
                    return Skip.Cancel;
                }
            }
            return job;
        }

        private JobDto GetBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var types = new List<JobTypeEnums>()
            {
                JobTypeEnums.NormalBuild,
                JobTypeEnums.ResourceBuild
            };
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => types.Contains(x.Type))
                .OrderBy(x => x.Position)
                .ToDto()
                .FirstOrDefault();
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

        private Result IsJobValid(VillageId villageId, NormalBuildPlan plan)
        {
            using var context = _contextFactory.CreateDbContext();

            var currentBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .FirstOrDefault();

            if (currentBuilding is not null && currentBuilding.Type == plan.Type) return Result.Ok();

            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            var errors = new List<PrerequisiteBuildingMissing>();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = context.Buildings
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Type == prerequisiteBuilding.Type)
                    .Any(x => x.Level >= prerequisiteBuilding.Level);
                if (!vaild) errors.Add(new(prerequisiteBuilding.Type, prerequisiteBuilding.Level, false));
            }

            if (!plan.Type.IsMultipleBuilding()) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();

            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (building is null) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
            if (building.Level == building.Type.GetMaxLevel()) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();

            errors.Add(new(building.Type, building.Level, false));
            return Result.Fail(errors);
        }
    }
}