using MainCore.Commands.Abstract;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [RegisterScoped<HandleJobCommand>]
    public class HandleJobCommand : CommandBase
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly JobUpdated.Handler _jobUpdated;

        private readonly ToDorfCommand _toDorfCommand;
        private readonly UpdateBuildingCommand _updateBuildingCommand;
        private readonly IGetSetting _getSetting;
        private readonly GetLayoutBuildingsQuery.Handler _getLayoutBuildingsQuery;

        private static readonly List<BuildingEnums> _resourceTypes =
        [
            BuildingEnums.Woodcutter,
            BuildingEnums.ClayPit,
            BuildingEnums.IronMine,
            BuildingEnums.Cropland
        ];

        private static readonly Dictionary<ResourcePlanEnums, List<BuildingEnums>> _fieldList = new()
        {
            {ResourcePlanEnums.AllResources, _resourceTypes},
            {ResourcePlanEnums.ExcludeCrop, _resourceTypes.Take(3).ToList()},
            {ResourcePlanEnums.OnlyCrop, _resourceTypes.Skip(3).ToList()},
        };

        public HandleJobCommand(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory, ToDorfCommand toDorfCommand, UpdateBuildingCommand updateBuildingCommand, IGetSetting getSetting, GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery, JobUpdated.Handler jobUpdated) : base(dataService)
        {
            _contextFactory = contextFactory;
            _toDorfCommand = toDorfCommand;
            _updateBuildingCommand = updateBuildingCommand;
            _getSetting = getSetting;
            _getLayoutBuildingsQuery = getLayoutBuildingsQuery;
            _jobUpdated = jobUpdated;
        }

        public async Task<Result<NormalBuildPlan>> Execute(CancellationToken cancellationToken)
        {
            var (_, isFailed, job, errors) = await GetJob(cancellationToken);
            if (isFailed) return Result.Fail(errors).WithError(TraceMessage.Error(TraceMessage.Line()));

            Result result;
            if (job.Type == JobTypeEnums.ResourceBuild)
            {
                result = await ExtractResourceFieldJobCommand(job, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                return Continue.Error;
            }

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            var dorf = plan.Location < 19 ? 1 : 2;
            result = await _toDorfCommand.Execute(dorf, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            if (await JobComplete(job))
            {
                return Continue.Error;
            }

            return plan;
        }

        #region GetJob

        public async Task<Result<JobDto>> GetJob(CancellationToken cancellationToken)
        {
            var countJob = CountBuildingJob();
            if (countJob == 0) return Skip.AutoBuilderJobQueueEmpty;

            var countQueueBuilding = CountQueueBuilding();
            if (countQueueBuilding == 0)
            {
                var result = await GetBuildingJob(false, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                return result.Value;
            }

            var plusActive = IsPlusActive();
            var applyRomanQueueLogic = _getSetting.BooleanByName(_dataService.VillageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (countQueueBuilding == 1)
            {
                if (plusActive)
                {
                    var result = await GetBuildingJob(false, cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    return result.Value;
                }

                if (applyRomanQueueLogic)
                {
                    var result = await GetBuildingJob(true, cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    return result.Value;
                }

                return BuildingQueueFull.Error;
            }

            if (countQueueBuilding == 2)
            {
                if (plusActive && applyRomanQueueLogic)
                {
                    var result = await GetBuildingJob(true, cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    return result.Value;
                }
                return BuildingQueueFull.Error;
            }

            return BuildingQueueFull.Error;
        }

        private async Task<Result<JobDto>> GetBuildingJob(bool romanLogic, CancellationToken cancellationToken)
        {
            var job = romanLogic ? GetJobBasedOnRomanLogic() : GetBuildingJob();
            if (job is null) return BuildingQueueFull.Error;

            if (job.Type == JobTypeEnums.ResourceBuild) return job;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
            if (plan.Type.IsResourceField()) return job;

            var valid = IsJobValid(plan);

            if (!valid.IsFailed) return job;

            Result result;
            result = await _toDorfCommand.Execute(2, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            valid = IsJobValid(plan);

            if (!valid.IsFailed) return job;

            var logger = _dataService.Logger;
            logger.Warning("Try to build {Type} level {Level} but", plan.Type, plan.Level);
            foreach (var error in valid.Errors.OfType<PrerequisiteBuildingMissing>())
            {
                error.Log(logger);
            }

            return BuildingQueueFull.Error;
        }

        private JobDto GetBuildingJob()
        {
            var villageId = _dataService.VillageId;
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

        #region RomanLogic

        private JobDto GetJobBasedOnRomanLogic()
        {
            var countQueueBuilding = CountQueueBuilding();
            var countResourceQueueBuilding = CountResourceQueueBuilding();
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;
            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                return GetInfrastructureBuildingJob();
            }
            else
            {
                return GetResourceBuildingJob();
            }
        }

        private int CountQueueBuilding()
        {
            var villageId = _dataService.VillageId;
            using var context = _contextFactory.CreateDbContext();
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }

        private int CountResourceQueueBuilding()
        {
            var villageId = _dataService.VillageId;
            using var context = _contextFactory.CreateDbContext();

            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => _resourceTypes.Contains(x.Type))
                .Count();
            return count;
        }

        private JobDto GetInfrastructureBuildingJob()
        {
            var villageId = _dataService.VillageId;
            using var context = _contextFactory.CreateDbContext();

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
                .Where(x => !_resourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return job;
        }

        private JobDto GetResourceBuildingJob()
        {
            var villageId = _dataService.VillageId;
            using var context = _contextFactory.CreateDbContext();

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
                .Where(x => _resourceTypes.Contains(x.Content.Type))
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

        #endregion RomanLogic

        private int CountBuildingJob()
        {
            var villageId = _dataService.VillageId;
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

        private Result IsJobValid(NormalBuildPlan plan)
        {
            var villageId = _dataService.VillageId;
            using var context = _contextFactory.CreateDbContext();

            var currentBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .FirstOrDefault();

            if (currentBuilding is not null && currentBuilding.Type == plan.Type) return Result.Ok();

            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            var errors = new List<PrerequisiteBuildingMissing>();

            var buildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => prerequisiteBuildings.Select(x => x.Type).Contains(x.Type))
                .ToList();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = buildings
                    .Where(x => x.Type == prerequisiteBuilding.Type)
                    .Any(x => x.Level >= prerequisiteBuilding.Level);
                if (!vaild) errors.Add(new(prerequisiteBuilding.Type, prerequisiteBuilding.Level));
            }

            if (!plan.Type.IsMultipleBuilding()) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();

            var firstBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (firstBuilding is null) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
            if (firstBuilding.Level == firstBuilding.Type.GetMaxLevel()) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();

            errors.Add(new(firstBuilding.Type, firstBuilding.Level));
            return Result.Fail(errors);
        }

        private bool IsPlusActive()
        {
            var html = _dataService.ChromeBrowser.Html;

            return InfoParser.HasPlusAccount(html);
        }

        #endregion GetJob

        #region Extract resource field job

        public async Task<Result> ExtractResourceFieldJobCommand(JobDto job, CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var villageId = _dataService.VillageId;
            var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);

            var normalBuildPlan = await GetNormalBuildPlan(resourceBuildPlan);
            if (normalBuildPlan is null)
            {
                var deleteJobByIdCommand = Locator.Current.GetService<DeleteJobByIdCommand.Handler>();
                await deleteJobByIdCommand.HandleAsync(new(accountId, villageId, job.Id), cancellationToken);
            }
            else
            {
                var addJobCommand = Locator.Current.GetService<AddJobCommand.Handler>();
                await addJobCommand.HandleAsync(new(accountId, villageId, normalBuildPlan.ToJob(villageId), true));
            }
            return Result.Ok();
        }

        private async Task<NormalBuildPlan> GetNormalBuildPlan(ResourceBuildPlan plan)
        {
            var villageId = _dataService.VillageId;
            var resourceTypes = _fieldList[plan.Plan];

            var buildings = await _getLayoutBuildingsQuery.HandleAsync(new(villageId, true));

            buildings = buildings
                .Where(x => resourceTypes.Contains(x.Type))
                .Where(x => x.Level < plan.Level)
                .ToList();

            if (buildings.Count == 0) return null;

            var minLevel = buildings
                .Select(x => x.Level)
                .Min();

            var chosenOne = buildings
                .Where(x => x.Level == minLevel)
                .OrderBy(x => x.Id.Value + Random.Shared.Next())
                .FirstOrDefault();

            if (chosenOne is null) return null;

            var normalBuildPlan = new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = chosenOne.Level + 1,
                Location = chosenOne.Location,
            };
            return normalBuildPlan;
        }

        #endregion Extract resource field job

        #region Check job complete

        private async Task<bool> JobComplete(JobDto job)
        {
            if (IsJobComplete(job))
            {
                var deleteJobByIdCommand = Locator.Current.GetService<DeleteJobByIdCommand.Handler>();
                await deleteJobByIdCommand.HandleAsync(new(_dataService.AccountId, _dataService.VillageId, job.Id));
                return true;
            }
            return false;
        }

        private bool IsJobComplete(JobDto job)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return false;
            var villageId = _dataService.VillageId;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            using var context = _contextFactory.CreateDbContext();

            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .OrderByDescending(x => x.Level)
                .Select(x => x.Level)
                .FirstOrDefault();

            if (queueBuilding >= plan.Level) return true;

            var villageBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .Select(x => x.Level)
                .FirstOrDefault();
            if (villageBuilding >= plan.Level) return true;

            return false;
        }

        #endregion Check job complete
    }
}