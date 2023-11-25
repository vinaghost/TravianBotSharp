using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpgradeBuildingTask : VillageTask
    {
        private readonly IUnitOfCommand _unitOfCommand;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

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

        public UpgradeBuildingTask(IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository, IChooseBuildingJobCommand chooseBuildingJobCommand, IExtractResourceFieldCommand extractResourceFieldCommand, IToBuildingPageCommand toBuildingPageCommand, IGetRequiredResourceCommand getRequiredResourceCommand, IAddCroplandCommand addCroplandCommand, IGetTimeWhenEnoughResourceCommand getTimeWhenEnoughResourceCommand, IConstructCommand constructCommand, IUpgradeCommand upgradeCommand, ISpecialUpgradeCommand specialUpgradeCommand, IUseHeroResourceCommand useHeroResourceCommand, IMediator mediator)
        {
            _unitOfCommand = unitOfCommand;
            _unitOfRepository = unitOfRepository;
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
            _mediator = mediator;
        }

        protected override  async Task<Result> Execute()
        {
            if (CancellationToken.IsCancellationRequested) return new Cancel();
            Result result;

            result = _unitOfCommand.SwitchVillageCommand.Execute(AccountId, VillageId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

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
                    return result.WithError(new TraceMessage(TraceMessage.Line()));
                }
                var job = _chooseBuildingJobCommand.Value;
                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    result = await _extractResourceFieldCommand.Execute(AccountId, VillageId, job);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
                result = await _toBuildingPageCommand.Execute(AccountId, VillageId, plan);
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
                        var heroResourceResult = await _useHeroResourceCommand.Execute(AccountId, missingResource);
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
                    if (IsSpecialUpgrade())
                    {
                        result = await _specialUpgradeCommand.Execute(AccountId);
                        if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    }
                    else
                    {
                        result = _upgradeCommand.Execute(AccountId);
                        if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    }
                }
                else
                {
                    result = _constructCommand.Execute(AccountId, plan);
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