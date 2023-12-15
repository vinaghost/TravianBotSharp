using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
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
        private readonly ICommandHandler<ChooseBuildingJobCommand, JobDto> _chooseBuildingJobCommand;
        private readonly ICommandHandler<ExtractResourceFieldCommand> _extractResourceFieldCommand;
        private readonly ICommandHandler<ToBuildingPageCommand> _toBuildingPageCommand;
        private readonly ICommandHandler<GetRequiredResourceCommand, long[]> _getRequiredResourceCommand;
        private readonly ICommandHandler<AddCroplandCommand> _addCroplandCommand;
        private readonly ICommandHandler<GetTimeWhenEnoughResourceCommand, TimeSpan> _getTimeWhenEnoughResourceCommand;
        private readonly ICommandHandler<ConstructCommand> _constructCommand;
        private readonly ICommandHandler<UpgradeCommand> _upgradeCommand;
        private readonly ICommandHandler<SpecialUpgradeCommand> _specialUpgradeCommand;

        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;

        private readonly ILogService _logService;
        private readonly ITaskManager _taskManager;

        public UpgradeBuildingTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<ChooseBuildingJobCommand, JobDto> chooseBuildingJobCommand, ICommandHandler<ExtractResourceFieldCommand> extractResourceFieldCommand, ICommandHandler<ToBuildingPageCommand> toBuildingPageCommand, ICommandHandler<GetRequiredResourceCommand, long[]> getRequiredResourceCommand, ICommandHandler<AddCroplandCommand> addCroplandCommand, ICommandHandler<GetTimeWhenEnoughResourceCommand, TimeSpan> getTimeWhenEnoughResourceCommand, ICommandHandler<ConstructCommand> constructCommand, ICommandHandler<UpgradeCommand> upgradeCommand, ICommandHandler<SpecialUpgradeCommand> specialUpgradeCommand, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand, ILogService logService, ITaskManager taskManager) : base(unitOfCommand, unitOfRepository, mediator)
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
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            var logger = _logService.GetLogger(AccountId);

            Result result;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return new Cancel();

                result = await _unitOfCommand.UpdateAccountInfoCommand.Handle(new(AccountId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.UpdateDorfCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _chooseBuildingJobCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<BuildingQueue>())
                    {
                        await SetTimeQueueBuildingComplete(AccountId, VillageId);
                    }
                    return result;
                }
                var job = _chooseBuildingJobCommand.Value;
                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    result = await _extractResourceFieldCommand.Handle(new(AccountId, VillageId, job), CancellationToken);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

                var dorf = plan.Location < 19 ? 1 : 2;
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, dorf), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.UpdateDorfCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                if (await JobComplete(AccountId, VillageId, job))
                {
                    continue;
                }

                logger.Information("Build {type} to level {level} at location {location}", plan.Type, plan.Level, plan.Location);
                result = await _toBuildingPageCommand.Handle(new(AccountId, VillageId, plan), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _getRequiredResourceCommand.Handle(new(AccountId, VillageId, plan), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                var requiredResource = _getRequiredResourceCommand.Value;
                result = _unitOfRepository.StorageRepository.IsEnoughResource(VillageId, requiredResource);
                if (result.IsFailed)
                {
                    if (result.HasError<FreeCrop>())
                    {
                        result = await _addCroplandCommand.Handle(new(AccountId, VillageId), CancellationToken);
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
                        var heroResourceResult = await _useHeroResourceCommand.Handle(new(AccountId, missingResource), CancellationToken);
                        if (heroResourceResult.IsFailed)
                        {
                            if (!heroResourceResult.HasError<Retry>())
                            {
                                var timeResult = await SetEnoughResourcesTime(AccountId, VillageId, plan);
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
                        result = await _specialUpgradeCommand.Handle(new(AccountId), CancellationToken);
                        if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    }
                    else
                    {
                        result = await _upgradeCommand.Handle(new(AccountId), CancellationToken);
                        if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    }
                }
                else
                {
                    result = await _constructCommand.Handle(new(AccountId, plan), CancellationToken);
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

        private async Task<Result> SetEnoughResourcesTime(AccountId accountId, VillageId villageId, NormalBuildPlan plan)
        {
            var result = await _getTimeWhenEnoughResourceCommand.Handle(new(accountId, villageId, plan), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            var time = _getTimeWhenEnoughResourceCommand.Value;
            ExecuteAt = DateTime.Now.Add(time);
            await _taskManager.ReOrder(accountId);
            return Result.Ok();
        }

        private async Task SetTimeQueueBuildingComplete(AccountId accountId, VillageId villageId)
        {
            var buildingQueue = _unitOfRepository.QueueBuildingRepository.GetFirst(villageId);
            if (buildingQueue is null)
            {
                ExecuteAt = DateTime.Now.AddSeconds(1);
                await _taskManager.ReOrder(accountId);
                return;
            }

            ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
            await _taskManager.ReOrder(accountId);
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