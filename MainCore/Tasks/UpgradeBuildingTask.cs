using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using OpenQA.Selenium;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpgradeBuildingTask : VillageTask
    {
        private readonly ICommandHandler<SpecialUpgradeCommand> _specialUpgradeCommand;
        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;

        private readonly ILogService _logService;
        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public UpgradeBuildingTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<SpecialUpgradeCommand> specialUpgradeCommand, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand, ILogService logService, ITaskManager taskManager, IChromeManager chromeManager, UnitOfParser unitOfParser) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _specialUpgradeCommand = specialUpgradeCommand;
            _useHeroResourceCommand = useHeroResourceCommand;
            _logService = logService;
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        protected override async Task<Result> Execute()
        {
            var logger = _logService.GetLogger(AccountId);
            Result result;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return Cancel.Error;
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 0), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var resultJob = await GetBuildingJob();
                if (resultJob.IsFailed)
                {
                    if (resultJob.HasError<BuildingQueue>())
                    {
                        await SetTimeQueueBuildingComplete();
                    }
                    return Result.Fail(resultJob.Errors)
                        .WithError(TraceMessage.Error(TraceMessage.Line()));
                }

                var job = resultJob.Value;

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    result = await ExtractResourceField(job);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

                var dorf = plan.Location < 19 ? 1 : 2;
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, dorf), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                if (await JobComplete(job))
                {
                    continue;
                }

                logger.Information("Build {type} to level {level} at location {location}", plan.Type, plan.Level, plan.Location);

                result = await ToBuildingPage(plan);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var requiredResource = GetRequiredResource(plan);

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
                        var heroResourceResult = await _useHeroResourceCommand.Handle(new(AccountId, missingResource), CancellationToken);
                        if (heroResourceResult.IsFailed)
                        {
                            if (!heroResourceResult.HasError<Retry>())
                            {
                                await SetEnoughResourcesTime(plan);
                            }

                            return heroResourceResult.WithError(TraceMessage.Error(TraceMessage.Line()));
                        }
                    }
                    else
                    {
                        await SetEnoughResourcesTime(plan);
                        return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }

                if (IsUpgradeable(plan))
                {
                    if (IsSpecialUpgrade() && IsSpecialUpgradeable(plan))
                    {
                        result = await _specialUpgradeCommand.Handle(new(AccountId), CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                    else
                    {
                        result = await Upgrade();
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }
                else
                {
                    result = await Construct(plan);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
            }
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Upgrade building in {village}";
        }

        private bool IsUpgradeable(NormalBuildPlan plan)
        {
            var emptySite = _unitOfRepository.BuildingRepository.EmptySite(VillageId, plan.Location);
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
                var dto = _unitOfRepository.BuildingRepository.GetBuilding(VillageId, plan.Location);
                if (dto.Level == 0) return false;
            }
            return true;
        }

        private bool IsSpecialUpgrade()
        {
            var useSpecialUpgrade = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseSpecialUpgrade);
            return useSpecialUpgrade;
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private async Task SetEnoughResourcesTime(NormalBuildPlan plan)
        {
            var time = GetTimeWhenEnoughResource(plan);
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

        #region get building job

        private async Task<Result<JobDto>> GetBuildingJob()
        {
            do
            {
                var countJob = _unitOfRepository.JobRepository.CountBuildingJob(VillageId);

                if (countJob == 0)
                {
                    return Skip.AutoBuilderJobQueueEmpty;
                }

                var countQueueBuilding = _unitOfRepository.BuildingRepository.CountQueueBuilding(VillageId);

                if (countQueueBuilding == 0)
                {
                    var job = await GetBuildingJob(false);
                    if (job.IsFailed)
                    {
                        if (job.HasError<JobCompleted>()) continue;

                        return Result.Fail(job.Errors)
                            .WithError(TraceMessage.Error(TraceMessage.Line()));
                    }

                    Result result;
                    result = _unitOfRepository.JobRepository.JobValid(VillageId, job.Value);
                    if (result.IsFailed)
                    {
                        result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                        result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                        result = _unitOfRepository.JobRepository.JobValid(VillageId, job.Value);
                        if (result.IsFailed) return Result.Fail(job.Errors)
                            .WithError(TraceMessage.Error(TraceMessage.Line()))
                            .WithError(Stop.AutoBuilderQueueInvalid);
                    }
                    return job;
                }

                var plusActive = _unitOfRepository.AccountInfoRepository.IsPlusActive(AccountId);
                var applyRomanQueueLogic = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

                if (countQueueBuilding == 1)
                {
                    if (plusActive)
                    {
                        var job = await GetBuildingJob(false);
                        if (job.IsFailed)
                        {
                            if (job.HasError<JobCompleted>()) continue;
                            return Result.Fail(job.Errors)
                                .WithError(TraceMessage.Error(TraceMessage.Line()));
                        }

                        var result = _unitOfRepository.JobRepository.JobValid(VillageId, job.Value);
                        if (result.IsFailed)
                        {
                            job = _unitOfRepository.JobRepository.GetResourceBuildingJob(VillageId);
                            if (job is null) return result;
                        }

                        return job;
                    }
                    if (applyRomanQueueLogic)
                    {
                        var job = await GetBuildingJob(true);
                        if (job.IsFailed)
                        {
                            if (job.HasError<JobCompleted>()) continue;
                            return Result.Fail(job.Errors)
                                .WithError(TraceMessage.Error(TraceMessage.Line()));
                        }

                        var result = _unitOfRepository.JobRepository.JobValid(VillageId, job.Value);
                        if (result.IsFailed)
                        {
                            job = _unitOfRepository.JobRepository.GetResourceBuildingJob(VillageId);
                            if (job is null) return result;
                        }

                        return job;
                    }
                    return BuildingQueue.Full;
                }

                if (countQueueBuilding == 2)
                {
                    if (plusActive && applyRomanQueueLogic)
                    {
                        var job = await GetBuildingJob(false);
                        if (job.IsFailed)
                        {
                            if (job.HasError<JobCompleted>()) continue;
                            return Result.Fail(job.Errors)
                                .WithError(TraceMessage.Error(TraceMessage.Line()));
                        }
                        return job;
                    }
                    return BuildingQueue.Full;
                }
                return BuildingQueue.Full;
            }
            while (true);
        }

        private async Task<Result<JobDto>> GetBuildingJob(bool romanLogic)
        {
            JobDto job;

            if (romanLogic)
            {
                job = GetJobBasedOnRomanLogic();
            }
            else
            {
                job = _unitOfRepository.JobRepository.GetBuildingJob(VillageId);
            }

            if (await JobComplete(job))
            {
                return JobCompleted.Error;
            }

            return job;
        }

        private JobDto GetJobBasedOnRomanLogic()
        {
            var countQueueBuilding = _unitOfRepository.BuildingRepository.CountQueueBuilding(VillageId);
            var countResourceQueueBuilding = _unitOfRepository.BuildingRepository.CountResourceQueueBuilding(VillageId);
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;
            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                return _unitOfRepository.JobRepository.GetInfrastructureBuildingJob(VillageId);
            }
            else
            {
                return _unitOfRepository.JobRepository.GetResourceBuildingJob(VillageId);
            }
        }

        #endregion get building job

        public async Task<Result> ExtractResourceField(JobDto job)
        {
            var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);
            var normalBuildPlan = _unitOfRepository.BuildingRepository.GetNormalBuildPlan(VillageId, resourceBuildPlan);
            if (normalBuildPlan is null)
            {
                _unitOfRepository.JobRepository.Delete(job.Id);
            }
            else
            {
                _unitOfRepository.JobRepository.AddToTop(VillageId, normalBuildPlan);
            }
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
            return Result.Ok();
        }

        public async Task AddCropland()
        {
            var cropland = _unitOfRepository.BuildingRepository.GetCropland(VillageId);

            var plan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };

            _unitOfRepository.JobRepository.AddToTop(VillageId, plan);
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        public async Task<Result> Construct(NormalBuildPlan plan)
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var button = _unitOfParser.UpgradeBuildingParser.GetConstructButton(html, plan.Type);
            if (button is null) return Retry.ButtonNotFound("construct");

            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("dorf", CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        public async Task<Result> Upgrade()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var button = _unitOfParser.UpgradeBuildingParser.GetUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("upgrade");

            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("dorf", CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        public async Task<Result> ToBuildingPage(NormalBuildPlan plan)
        {
            Result result;

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, plan.Location), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var building = _unitOfRepository.BuildingRepository.GetBuilding(VillageId, plan.Location);
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = plan.Type.GetBuildingsCategory();
                result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, tabIndex), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                if (building.Level == 0) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();
                result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, 0), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        public long[] GetRequiredResource(NormalBuildPlan plan)
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var isEmptySite = _unitOfRepository.BuildingRepository.EmptySite(VillageId, plan.Location);
            return _unitOfParser.UpgradeBuildingParser.GetRequiredResource(html, isEmptySite, plan.Type);
        }

        public TimeSpan GetTimeWhenEnoughResource(NormalBuildPlan plan)
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;
            var isEmptySite = _unitOfRepository.BuildingRepository.EmptySite(VillageId, plan.Location);
            return _unitOfParser.UpgradeBuildingParser.GetTimeWhenEnoughResource(html, isEmptySite, plan.Type);
        }
    }
}