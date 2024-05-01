using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Commands.Misc;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.DTO;
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

        public UpgradeBuildingTask(IChromeManager chromeManager, UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IVillageRepository villageRepository, ILogService logService, ITaskManager taskManager, IUpgradeBuildingParser upgradeBuildingParser, IBuildingRepository buildingRepository, IVillageSettingRepository villageSettingRepository) : base(chromeManager, unitOfCommand, unitOfRepository, mediator, villageRepository)
        {
            _logService = logService;
            _taskManager = taskManager;
            _upgradeBuildingParser = upgradeBuildingParser;
            _buildingRepository = buildingRepository;
            _villageSettingRepository = villageSettingRepository;
        }

        protected override async Task<Result> Execute()
        {
            var logger = _logService.GetLogger(AccountId);
            Result result;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return Cancel.Error;

                result = await _mediator.Send(ToDorfCommand.ToDorf(AccountId), CancellationToken);
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
                result = await _mediator.Send(new ToDorfCommand(AccountId, dorf), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                if (await JobComplete(job))
                {
                    continue;
                }

                logger.Information("Build {type} to level {level} at location {location}", plan.Type, plan.Level, plan.Location);

                var chromeBrowser = _chromeManager.Get(AccountId);

                result = await ToBuildingPage(chromeBrowser, plan);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var requiredResource = GetRequiredResource(chromeBrowser, plan);

                result = _unitOfRepository.StorageRepository.IsEnoughResource(VillageId, requiredResource);
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
                        var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(VillageId, requiredResource);
                        var heroResourceResult = await _mediator.Send(new UseHeroResourceCommand(AccountId, chromeBrowser, missingResource), CancellationToken);
                        if (heroResourceResult.IsFailed)
                        {
                            if (!heroResourceResult.HasError<Retry>())
                            {
                                await SetEnoughResourcesTime(chromeBrowser, plan);
                            }

                            return heroResourceResult.WithError(TraceMessage.Error(TraceMessage.Line()));
                        }
                    }
                    else
                    {
                        await SetEnoughResourcesTime(chromeBrowser, plan);
                        return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }
                if (IsUpgradeable(plan))
                {
                    if (IsSpecialUpgrade() && IsSpecialUpgradeable(plan))
                    {
                        result = await _mediator.Send(new SpecialUpgradeCommand(AccountId, chromeBrowser), CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                    else
                    {
                        result = await _mediator.Send(new UpgradeCommand(chromeBrowser), CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }
                else
                {
                    result = await _mediator.Send(new ConstructCommand(chromeBrowser, plan.Type), CancellationToken);
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
            var useSpecialUpgrade = _villageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseSpecialUpgrade);
            return useSpecialUpgrade;
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = _villageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private async Task SetEnoughResourcesTime(IChromeBrowser chromeBrowser, NormalBuildPlan plan)
        {
            var time = GetTimeWhenEnoughResource(chromeBrowser, plan);
            ExecuteAt = DateTime.Now.Add(time);
            await _taskManager.ReOrder(AccountId);
        }

        private async Task SetTimeQueueBuildingComplete()
        {
            var buildingQueue = _unitOfRepository.QueueBuildingRepository.GetFirst(VillageId);
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
            if (_unitOfRepository.JobRepository.JobComplete(VillageId, job))
            {
                _unitOfRepository.JobRepository.Delete(job.Id);
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

            _unitOfRepository.JobRepository.AddToTop(VillageId, plan);
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        public async Task<Result> ToBuildingPage(IChromeBrowser chromeBrowser, NormalBuildPlan plan)
        {
            Result result;
            result = await _mediator.Send(new ToBuildingCommand(chromeBrowser, plan.Location), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var building = _buildingRepository.GetBuilding(VillageId, plan.Location);
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = plan.Type.GetBuildingsCategory();
                result = await _mediator.Send(new SwitchTabCommand(chromeBrowser, tabIndex), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                if (building.Level <= 0) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();
                result = await _mediator.Send(new SwitchTabCommand(chromeBrowser, 0), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        public long[] GetRequiredResource(IChromeBrowser chromeBrowser, NormalBuildPlan plan)
        {
            var html = chromeBrowser.Html;

            var isEmptySite = _buildingRepository.EmptySite(VillageId, plan.Location);
            return _upgradeBuildingParser.GetRequiredResource(html, isEmptySite, plan.Type);
        }

        public TimeSpan GetTimeWhenEnoughResource(IChromeBrowser chromeBrowser, NormalBuildPlan plan)
        {
            var html = chromeBrowser.Html;
            var isEmptySite = _buildingRepository.EmptySite(VillageId, plan.Location);
            return _upgradeBuildingParser.GetTimeWhenEnoughResource(html, isEmptySite, plan.Type);
        }
    }
}