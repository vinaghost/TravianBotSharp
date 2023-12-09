using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Features.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpgradeBuildingTask : VillageTask
    {
        private readonly IChooseBuildingJobCommand _chooseBuildingJobCommand;
        private readonly IExtractResourceFieldCommand _extractResourceFieldCommand;
        private readonly IToBuildingPageCommand _toBuildingPageCommand;
        private readonly IGetRequiredResourceCommand _getRequiredResourceCommand;
        private readonly IAddCroplandCommand _addCroplandCommand;
        private readonly IGetTimeWhenEnoughResourceCommand _getTimeWhenEnoughResourceCommand;
        private readonly IConstructCommand _constructCommand;
        private readonly IUpgradeCommand _upgradeCommand;
        private readonly ISpecialUpgradeCommand _specialUpgradeCommand;

        private readonly IUseHeroResourceCommand _useHeroResourceCommand;

        private readonly ILogService _logService;

        public UpgradeBuildingTask(IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository, IMediator mediator, IChooseBuildingJobCommand chooseBuildingJobCommand, IExtractResourceFieldCommand extractResourceFieldCommand, IToBuildingPageCommand toBuildingPageCommand, IGetRequiredResourceCommand getRequiredResourceCommand, IAddCroplandCommand addCroplandCommand, IGetTimeWhenEnoughResourceCommand getTimeWhenEnoughResourceCommand, IConstructCommand constructCommand, IUpgradeCommand upgradeCommand, ISpecialUpgradeCommand specialUpgradeCommand, IUseHeroResourceCommand useHeroResourceCommand, ILogService logService) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _chooseBuildingJobCommand = chooseBuildingJobCommand;
            _extractResourceFieldCommand = extractResourceFieldCommand;
            _toBuildingPageCommand = toBuildingPageCommand;
            _getRequiredResourceCommand = getRequiredResourceCommand;
            _addCroplandCommand = addCroplandCommand;
            _getTimeWhenEnoughResourceCommand = getTimeWhenEnoughResourceCommand;
            _constructCommand = constructCommand;
            _upgradeCommand = upgradeCommand;
            _specialUpgradeCommand = specialUpgradeCommand;
            _useHeroResourceCommand = useHeroResourceCommand;
            _logService = logService;
        }

        protected override async Task<Result> Execute()
        {
            var logger = _logService.GetLogger(AccountId);

            Result result;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return new Cancel();

                result = await _unitOfCommand.UpdateAccountInfoCommand.Execute(AccountId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.UpdateDorfCommand.Execute(AccountId, VillageId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _chooseBuildingJobCommand.Execute(AccountId, VillageId);
                if (result.IsFailed)
                {
                    if (result.HasError<BuildingQueue>())
                    {
                        SetTimeQueueBuildingComplete(VillageId);
                    }
                    return result;
                }
                var job = _chooseBuildingJobCommand.Value;
                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    result = await _extractResourceFieldCommand.Execute(AccountId, VillageId, job);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
                logger.Information("Build {type} to level {level} at {location}", plan.Type, plan.Level, plan.Location);
                result = await _toBuildingPageCommand.Execute(AccountId, VillageId, plan, CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                if (await JobComplete(AccountId, VillageId, job))
                {
                    continue;
                }

                result = _getRequiredResourceCommand.Execute(AccountId, VillageId, plan);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                var requiredResource = _getRequiredResourceCommand.Value;
                result = _unitOfRepository.StorageRepository.IsEnoughResource(VillageId, requiredResource);
                if (result.IsFailed)
                {
                    if (result.HasError<FreeCrop>())
                    {
                        result = await _addCroplandCommand.Execute(AccountId, VillageId);
                        if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                        continue;
                    }
                    else if (result.HasError<GranaryLimit>())
                    {
                        return result.WithError(new Stop("Please take a look on building job queue")).WithError(new TraceMessage(TraceMessage.Line()));
                    }
                    else if (result.HasError<WarehouseLimit>())
                    {
                        return result.WithError(new Stop("Please take a look on building job queue")).WithError(new TraceMessage(TraceMessage.Line()));
                    }

                    if (IsUseHeroResource())
                    {
                        var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(VillageId, _getRequiredResourceCommand.Value);
                        var heroResourceResult = await _useHeroResourceCommand.Execute(AccountId, missingResource, CancellationToken);
                        if (heroResourceResult.IsFailed)
                        {
                            if (!heroResourceResult.HasError<Retry>())
                            {
                                var timeResult = SetEnoughResourcesTime(AccountId, VillageId, plan);
                                if (timeResult.IsFailed)
                                {
                                    return timeResult.WithError(new TraceMessage(TraceMessage.Line()));
                                }
                            }

                            return heroResourceResult.WithError(new TraceMessage(TraceMessage.Line()));
                        }
                    }
                }

                if (IsUpgradeable(plan))
                {
                    if (IsSpecialUpgrade() && IsSpecialUpgradeable(plan))
                    {
                        result = await _specialUpgradeCommand.Execute(AccountId, CancellationToken);
                        if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    }
                    else
                    {
                        result = await _upgradeCommand.Execute(AccountId, CancellationToken);
                        if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    }
                }
                else
                {
                    result = await _constructCommand.Execute(AccountId, plan, CancellationToken);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
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

        private Result SetEnoughResourcesTime(AccountId accountId, VillageId villageId, NormalBuildPlan plan)
        {
            var result = _getTimeWhenEnoughResourceCommand.Execute(accountId, villageId, plan);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            var time = _getTimeWhenEnoughResourceCommand.Value;
            ExecuteAt = DateTime.Now.Add(time);
            return Result.Ok();
        }

        private void SetTimeQueueBuildingComplete(VillageId villageId)
        {
            var buildingQueue = _unitOfRepository.QueueBuildingRepository.GetFirst(villageId);
            if (buildingQueue is null)
            {
                ExecuteAt = DateTime.Now.AddSeconds(1);
                return;
            }

            ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
        }

        private async Task<bool> JobComplete(AccountId accountId, VillageId villageId, JobDto job)
        {
            if (_unitOfRepository.JobRepository.JobComplete(villageId, job))
            {
                _unitOfRepository.JobRepository.Delete(job.Id);
                await _mediator.Publish(new JobUpdated(accountId, villageId));
                return true;
            }
            return false;
        }
    }
}