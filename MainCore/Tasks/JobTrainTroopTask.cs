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
    public class JobTrainTroopTask : VillageTask
    {
        private readonly ICommandHandler<InputAmountTroopCommand> _inputAmountTroopCommand;
        private readonly ICommandHandler<UseHeroResourceCommand> _useHeroResourceCommand;
        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;

        public JobTrainTroopTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<InputAmountTroopCommand> inputAmountTroopCommand, ICommandHandler<UseHeroResourceCommand> useHeroResourceCommand, ITaskManager taskManager, IChromeManager chromeManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _inputAmountTroopCommand = inputAmountTroopCommand;
            _useHeroResourceCommand = useHeroResourceCommand;
            _taskManager = taskManager;
            _chromeManager = chromeManager;
        }

        protected override async Task<Result> Execute()
        {
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return new Cancel();

                var job = _unitOfRepository.JobRepository.GetFirst(VillageId);
                if (job is null || job.Type != JobTypeEnums.TrainTroop) return Result.Fail(new Skip("No train troop job"));

                Result result;
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                var waitProgressingBuild = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.TrainTroopWaitBuilding);
                if (waitProgressingBuild)
                {
                    var progressingBuild = _unitOfRepository.QueueBuildingRepository.Count(VillageId);
                    if (progressingBuild > 0)
                    {
                        await SetTimeQueueBuildingComplete();
                        return Result.Fail(new Skip("Wait progressing build complete"));
                    }
                }

                var plan = JsonSerializer.Deserialize<TrainTroopPlan>(job.Content);

                var building = plan.Type.GetTrainBuilding();
                if (plan.Great) building = building.GetGreat();

                var location = _unitOfRepository.BuildingRepository.GetBuildingLocation(VillageId, building);
                if (location == default)
                {
                    return Result.Fail(new MissingBuilding(building))
                        .WithError(new Stop("reason below"));
                }

                var cost = plan.Type.GetTrainCost();
                cost = cost.Select(x => x * plan.Amount).ToArray();

                result = _unitOfRepository.StorageRepository.IsEnoughResource(VillageId, cost);
                if (result.IsFailed)
                {
                    var batch = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.TrainTroopBatch);
                    if (batch)
                    {
                        var batchSize = _unitOfRepository.VillageSettingRepository.GetByName(VillageId, VillageSettingEnums.TrainTroopBatchSize);

                        if (plan.Amount > batchSize)
                        {
                            var maximumTroop = _unitOfRepository.StorageRepository.GetMaximumTroopCanTrain(VillageId, plan.Type);
                            if (maximumTroop < batchSize) maximumTroop = batchSize;
                            var subPlan = new TrainTroopPlan()
                            {
                                Amount = maximumTroop,
                                Great = plan.Great,
                                Type = plan.Type,
                            };

                            plan.Amount -= maximumTroop;

                            _unitOfRepository.JobRepository.Update(job.Id, plan);
                            _unitOfRepository.JobRepository.AddToTop(VillageId, subPlan);
                            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
                            continue;
                        }
                    }

                    if (result.HasError<GranaryLimit>() || result.HasError<WarehouseLimit>())
                    {
                        return result
                            .WithError(new Stop("Please take a look on building job queue"))
                            .WithError(new TraceMessage(TraceMessage.Line()));
                    }

                    if (!IsUseHeroResource())
                    {
                        await SetEnoughResourcesTime(cost);
                        return result
                            .WithError(new TraceMessage(TraceMessage.Line()));
                    }

                    var missingResource = _unitOfRepository.StorageRepository.GetMissingResource(VillageId, cost);
                    var heroResourceResult = await _useHeroResourceCommand.Handle(new(AccountId, missingResource), CancellationToken);
                    if (heroResourceResult.IsFailed)
                    {
                        if (!heroResourceResult.HasError<Retry>())
                        {
                            await SetEnoughResourcesTime(cost);
                        }

                        return heroResourceResult.WithError(new TraceMessage(TraceMessage.Line()));
                    }
                }
                result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, location), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _inputAmountTroopCommand.Handle(new(AccountId, plan.Type, plan.Amount), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                _unitOfRepository.JobRepository.Delete(job.Id);
            }
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

        private async Task SetEnoughResourcesTime(long[] required)
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var time = chromeBrowser.GetTimeEnoughResource(required);
            ExecuteAt = DateTime.Now.Add(time);
            if (_taskManager.IsExist<UpgradeBuildingTask>(AccountId, VillageId))
            {
                var task = _taskManager.Get<UpgradeBuildingTask>(AccountId, VillageId);
                task.ExecuteAt = ExecuteAt.AddSeconds(1);
            }
            await _taskManager.ReOrder(AccountId);
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
    }
}