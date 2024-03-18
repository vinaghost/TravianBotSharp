using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.TrainTroop;
using MainCore.Commands.Features.Step.UpgradeBuilding;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class JobTrainTroopTask : VillageTask
    {
        private readonly ICommandHandler<InputAmountTroopCommand> _inputAmountTroopCommand;
        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;
        private readonly ITaskManager _taskManager;

        public JobTrainTroopTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<InputAmountTroopCommand> inputAmountTroopCommand, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand, ITaskManager taskManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _inputAmountTroopCommand = inputAmountTroopCommand;
            _useHeroResourceCommand = useHeroResourceCommand;
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            await Task.CompletedTask;

            var job = _unitOfRepository.JobRepository.GetFirst(VillageId);
            if (job.Type != JobTypeEnums.TrainTroop) return Result.Fail(new Skip("No train troop job"));

            var plan = JsonSerializer.Deserialize<TrainTroopPlan>(job.Content);

            var building = plan.Type.GetTrainBuilding();
            if (plan.Great) building = building.GetGreat();

            Result result;
            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var location = _unitOfRepository.BuildingRepository.GetBuildingLocation(VillageId, building);
            if (location == default)
            {
                return Result.Fail(new MissingBuilding(building))
                    .WithError(new Stop("reason below"));
            }

            var cost = plan.Type.GetTrainCost();
            Array.ForEach(cost, x => x *= plan.Amount);

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
                    await SetEnoughResourcesTime(AccountId);
                    return result.WithError(new TraceMessage(TraceMessage.Line()));
                }

                var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(VillageId, cost);
                var heroResourceResult = await _useHeroResourceCommand.Handle(new(AccountId, missingResource), CancellationToken);
                if (heroResourceResult.IsFailed)
                {
                    if (!heroResourceResult.HasError<Retry>())
                    {
                        await SetEnoughResourcesTime(AccountId);
                    }

                    return heroResourceResult.WithError(new TraceMessage(TraceMessage.Line()));
                }
            }
            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, location), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _inputAmountTroopCommand.Handle(new(AccountId, plan.Type, plan.Amount), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            _unitOfRepository.JobRepository.Delete(job.Id);

            return Result.Ok();
        }

        protected override void SetName()
        {
            var name = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Job training troop in {name}";
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private async Task SetEnoughResourcesTime(AccountId accountId)
        {
            ExecuteAt = DateTime.Now.AddMinutes(15);
            await _taskManager.ReOrder(accountId);
        }
    }
}