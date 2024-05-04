using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Models;
using MainCore.Tasks.Base;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpgradeBuildingTask : VillageTask
    {
        private readonly ILogService _logService;
        private readonly ITaskManager _taskManager;

        private readonly IUpgradeBuildingParser _upgradeBuildingParser;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IVillageSettingRepository _villageSettingRepository;

        private readonly IStorageRepository _storageRepository;
        private readonly IQueueBuildingRepository _queueBuildingRepository;
        private readonly IJobRepository _jobRepository;

        public UpgradeBuildingTask(IMediator mediator, IVillageRepository villageRepository, ILogService logService, ITaskManager taskManager, IUpgradeBuildingParser upgradeBuildingParser, IBuildingRepository buildingRepository, IVillageSettingRepository villageSettingRepository, IStorageRepository storageRepository, IQueueBuildingRepository queueBuildingRepository, IJobRepository jobRepository) : base(mediator, villageRepository)
        {
            _logService = logService;
            _taskManager = taskManager;
            _upgradeBuildingParser = upgradeBuildingParser;
            _buildingRepository = buildingRepository;
            _villageSettingRepository = villageSettingRepository;
            _storageRepository = storageRepository;
            _queueBuildingRepository = queueBuildingRepository;
            _jobRepository = jobRepository;
        }

        protected override async Task<Result> Execute()
        {
            var logger = _logService.GetLogger(AccountId);
            Result result;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return Cancel.Error;

                result = await new ToDorfCommand().Execute(_chromeBrowser, 0, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var jobResult = await _mediator.Send(new GetJobCommand(AccountId, VillageId), CancellationToken);

                if (jobResult.IsFailed)
                {
                    if (jobResult.HasError<BuildingQueue>())
                    {
                        await SetTimeQueueBuildingComplete();
                        return Skip.BuildingQueueFull;
                    }

                    return Result.Fail(jobResult.Errors)
                        .WithError(Skip.Cancel)
                        .WithError(TraceMessage.Error(TraceMessage.Line()));
                }

                var job = jobResult.Value;

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    result = await _mediator.Send(new ExtractResourceFieldJobCommand(AccountId, VillageId, job));
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

                var dorf = plan.Location < 19 ? 1 : 2;
                result = await new ToDorfCommand().Execute(_chromeBrowser, dorf, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                if (await JobComplete(job))
                {
                    continue;
                }

                logger.Information("Build {type} to level {level} at location {location}", plan.Type, plan.Level, plan.Location);

                result = await ToBuildingPage(_chromeBrowser, plan);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var requiredResource = GetRequiredResource(_chromeBrowser, plan);

                result = _storageRepository.IsEnoughResource(VillageId, requiredResource);
                if (result.IsFailed)
                {
                    if (result.HasError<FreeCrop>())
                    {
                        await AddCropland();
                        continue;
                    }

                    if (result.HasError<GranaryLimit>() || result.HasError<WarehouseLimit>())
                    {
                        return result
                            .WithError(Stop.NotEnoughStorageCapacity)
                            .WithError(TraceMessage.Error(TraceMessage.Line()));
                    }

                    if (IsUseHeroResource())
                    {
                        var missingResource = _storageRepository.GetMissingResource(VillageId, requiredResource);
                        var heroResourceResult = await _mediator.Send(new UseHeroResourceCommand(AccountId, _chromeBrowser, missingResource), CancellationToken);
                        if (heroResourceResult.IsFailed)
                        {
                            if (!heroResourceResult.HasError<Retry>())
                            {
                                await SetEnoughResourcesTime(_chromeBrowser, plan);
                            }

                            return heroResourceResult.WithError(TraceMessage.Error(TraceMessage.Line()));
                        }
                    }
                    else
                    {
                        await SetEnoughResourcesTime(_chromeBrowser, plan);
                        return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }
                if (IsUpgradeable(plan))
                {
                    if (IsSpecialUpgrade() && IsSpecialUpgradeable(plan))
                    {
                        result = await _mediator.Send(new SpecialUpgradeCommand(AccountId, _chromeBrowser), CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                    else
                    {
                        result = await _mediator.Send(new UpgradeCommand(_chromeBrowser), CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }
                else
                {
                    result = await _mediator.Send(new ConstructCommand(_chromeBrowser, plan.Type), CancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
            }
        }

        protected override void SetName()
        {
            var village = _villageRepository.GetVillageName(VillageId);
            _name = $"Upgrade building in {village}";
        }

        private bool IsUpgradeable(NormalBuildPlan plan)
        {
            var emptySite = _buildingRepository.EmptySite(VillageId, plan.Location);
            return !emptySite;
        }

        private bool IsSpecialUpgradeable(NormalBuildPlan plan)
        {
            var buildings = new List<BuildingEnums>()
            {
                BuildingEnums.Residence,
                BuildingEnums.Palace,
                BuildingEnums.CommandCenter
            };

            if (buildings.Contains(plan.Type)) return false;

            if (plan.Type.IsResourceField())
            {
                var dto = _buildingRepository.GetBuilding(VillageId, plan.Location);
                if (dto.Level == 0) return false;
            }
            return true;
        }

        private bool IsSpecialUpgrade()
        {
            var useSpecialUpgrade = new GetVillageSetting().BooleanByName(VillageId, VillageSettingEnums.UseSpecialUpgrade);
            return useSpecialUpgrade;
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = new GetVillageSetting().BooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private async Task SetEnoughResourcesTime(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            var time = GetTimeWhenEnoughResource(_chromeBrowser, plan);
            ExecuteAt = DateTime.Now.Add(time);
            await _taskManager.ReOrder(AccountId);
        }

        private async Task SetTimeQueueBuildingComplete()
        {
            var buildingQueue = _queueBuildingRepository.GetFirst(VillageId);
            if (buildingQueue is null)
            {
                ExecuteAt = DateTime.Now.AddSeconds(1);
                await _taskManager.ReOrder(AccountId);
                return;
            }

            ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
            await _taskManager.ReOrder(AccountId);
        }

        private async Task<bool> JobComplete(JobDto job)
        {
            if (_jobRepository.JobComplete(VillageId, job))
            {
                _jobRepository.Delete(job.Id);
                await _mediator.Publish(new JobUpdated(AccountId, VillageId));
                return true;
            }
            return false;
        }

        public async Task AddCropland()
        {
            var cropland = _buildingRepository.GetCropland(VillageId);

            var plan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };

            _jobRepository.AddToTop(VillageId, plan);
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        public async Task<Result> ToBuildingPage(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            Result result;
            result = await new ToBuildingCommand().Execute(_chromeBrowser, plan.Location, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var building = _buildingRepository.GetBuilding(VillageId, plan.Location);
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = plan.Type.GetBuildingsCategory();
                result = await new SwitchTabCommand().Execute(_chromeBrowser, tabIndex, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                if (building.Level <= 0) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();
                result = await new SwitchTabCommand().Execute(_chromeBrowser, 0, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        public long[] GetRequiredResource(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            var html = _chromeBrowser.Html;

            var isEmptySite = _buildingRepository.EmptySite(VillageId, plan.Location);
            return _upgradeBuildingParser.GetRequiredResource(html, isEmptySite, plan.Type);
        }

        public TimeSpan GetTimeWhenEnoughResource(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            var html = _chromeBrowser.Html;
            var isEmptySite = _buildingRepository.EmptySite(VillageId, plan.Location);
            return _upgradeBuildingParser.GetTimeWhenEnoughResource(html, isEmptySite, plan.Type);
        }
    }
}