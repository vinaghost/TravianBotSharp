using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.ResearchTroop;
using MainCore.Commands.Features.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class JobResearchTroopTask : VillageTask
    {
        private readonly ICommandHandler<ResearchCommand> _researchCommand;
        private readonly ITaskManager _taskManager;
        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;

        public JobResearchTroopTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ITaskManager taskManager, ICommandHandler<ResearchCommand> researchCommand, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _taskManager = taskManager;
            _researchCommand = researchCommand;
            _useHeroResourceCommand = useHeroResourceCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            var job = _unitOfRepository.JobRepository.GetFirst(VillageId);
            if (job is null || job.Type != JobTypeEnums.ResearchTroop) return Result.Fail(new Skip("No holding research job"));

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var waitProgressingBuild = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.ResearchTroopWaitBuilding);
            if (waitProgressingBuild)
            {
                var progressingBuild = _unitOfRepository.QueueBuildingRepository.Count(VillageId);
                if (progressingBuild > 0)
                {
                    await SetTimeQueueBuildingComplete();
                    return Result.Fail(new Skip("Wait progressing build complete"));
                }
            }

            var plan = JsonSerializer.Deserialize<ResearchTroopPlan>(job.Content);

            var location = _unitOfRepository.BuildingRepository.GetBuildingLocation(VillageId, BuildingEnums.Academy);
            if (location == default)
            {
                return Result.Fail(new MissingBuilding(BuildingEnums.Academy))
                    .WithError(new Stop("reason below"));
            }

            var cost = plan.Type.GetResearchCost();
            result = _unitOfRepository.StorageRepository.IsEnoughResource(VillageId, cost);

            if (result.IsFailed)
            {
                if (result.HasError<GranaryLimit>() || result.HasError<WarehouseLimit>())
                {
                    return result
                        .WithError(new Stop("Please take a look on building job queue"))
                        .WithError(new TraceMessage(TraceMessage.Line()));
                }

                if (!IsUseHeroResource())
                {
                    await SetEnoughResourcesTime();
                    return result
                        .WithError(new TraceMessage(TraceMessage.Line()));
                }

                var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(VillageId, cost);
                var heroResourceResult = await _useHeroResourceCommand.Handle(new(AccountId, missingResource), CancellationToken);
                if (heroResourceResult.IsFailed)
                {
                    if (!heroResourceResult.HasError<Retry>())
                    {
                        await SetEnoughResourcesTime();
                    }

                    return heroResourceResult.WithError(new TraceMessage(TraceMessage.Line()));
                }
            }

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, location), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _researchCommand.Handle(new(AccountId, plan.Type), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override void SetName()
        {
            var name = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Job researching troop in {name}";
        }

        private async Task SetTimeQueueBuildingComplete()
        {
            var buildingQueue = _unitOfRepository.QueueBuildingRepository.GetFirst(VillageId);

            ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
            if (_taskManager.IsExist<UpgradeBuildingTask>(AccountId, VillageId))
            {
                var task = _taskManager.Get<UpgradeBuildingTask>(AccountId, VillageId);
                task.ExecuteAt = ExecuteAt.AddSeconds(1);
            }

            await _taskManager.ReOrder(AccountId);
        }

        private async Task SetEnoughResourcesTime()
        {
            ExecuteAt = DateTime.Now.AddMinutes(15);
            if (_taskManager.IsExist<UpgradeBuildingTask>(AccountId, VillageId))
            {
                var task = _taskManager.Get<UpgradeBuildingTask>(AccountId, VillageId);
                task.ExecuteAt = ExecuteAt.AddSeconds(1);
            }
            await _taskManager.ReOrder(AccountId);
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }
    }
}