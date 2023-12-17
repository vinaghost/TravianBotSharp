using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Extensions;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;
using System.Text.Json;

namespace MainCore.Commands.Features.Step.UpgradeBuilding
{
    public class ChooseBuildingJobCommand : ByAccountVillageIdBase, ICommand<JobDto>
    {
        public ChooseBuildingJobCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class ChooseBuildingJobCommandHandler : ICommandHandler<ChooseBuildingJobCommand, JobDto>
    {
        private readonly UnitOfRepository _unitOfRepository;

        private readonly IMediator _mediator;

        public ChooseBuildingJobCommandHandler(UnitOfRepository unitOfRepository, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public JobDto Value { get; private set; }

        public async Task<Result> Handle(ChooseBuildingJobCommand command, CancellationToken cancellationToken)
        {
            do
            {
                var countJob = _unitOfRepository.JobRepository.CountBuildingJob(command.VillageId);

                if (countJob == 0)
                {
                    return Result.Fail(Skip.BuildingJobQueueEmpty);
                }

                var countQueueBuilding = _unitOfRepository.BuildingRepository.CountQueueBuilding(command.VillageId);

                if (countQueueBuilding == 0)
                {
                    var job = _unitOfRepository.JobRepository.GetBuildingJob(command.VillageId);
                    if (await JobComplete(command.AccountId, command.VillageId, job)) continue;
                    Value = job;
                    return Result.Ok();
                }

                var plusActive = _unitOfRepository.AccountInfoRepository.IsPlusActive(command.AccountId);
                var applyRomanQueueLogic = _unitOfRepository.VillageSettingRepository.GetBooleanByName(command.VillageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

                if (countQueueBuilding == 1)
                {
                    if (plusActive)
                    {
                        var job = _unitOfRepository.JobRepository.GetBuildingJob(command.VillageId);
                        if (await JobComplete(command.AccountId, command.VillageId, job)) continue;
                        if (!JobRequirements(command.VillageId, job))
                        {
                            job = _unitOfRepository.JobRepository.GetResourceBuildingJob(command.VillageId);
                            if (job is null) return Result.Fail(BuildingQueue.NotEnoughPrerequisiteBuilding);
                        }
                        Value = job;
                        return Result.Ok();
                    }
                    if (applyRomanQueueLogic)
                    {
                        var job = GetJobBasedOnRomanLogic(command.VillageId, countQueueBuilding);
                        if (job is null) return Result.Fail(BuildingQueue.NotTaskInqueue);
                        if (await JobComplete(command.AccountId, command.VillageId, job)) continue;
                        if (!JobRequirements(command.VillageId, job)) return Result.Fail(BuildingQueue.NotEnoughPrerequisiteBuilding);
                        Value = job;
                        return Result.Ok();
                    }
                    return Result.Fail(BuildingQueue.Full);
                }

                if (countQueueBuilding == 2)
                {
                    if (applyRomanQueueLogic)
                    {
                        var job = GetJobBasedOnRomanLogic(command.VillageId, countQueueBuilding);
                        if (job is null) return Result.Fail(BuildingQueue.NotTaskInqueue);
                        if (await JobComplete(command.AccountId, command.VillageId, job)) continue;
                        if (!JobRequirements(command.VillageId, job)) return Result.Fail(BuildingQueue.NotEnoughPrerequisiteBuilding);
                        Value = job;
                        return Result.Ok();
                    }
                    return Result.Fail(BuildingQueue.Full);
                }
                return Result.Fail(BuildingQueue.Full);
            }
            while (true);
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

        private JobDto GetJobBasedOnRomanLogic(VillageId villageId, int countQueueBuilding)
        {
            var countResourceQueueBuilding = _unitOfRepository.BuildingRepository.CountResourceQueueBuilding(villageId);
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;
            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                return _unitOfRepository.JobRepository.GetInfrastructureBuildingJob(villageId);
            }
            else
            {
                return _unitOfRepository.JobRepository.GetResourceBuildingJob(villageId);
            }
        }

        private bool JobRequirements(VillageId villageId, JobDto job)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return true;
            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();
            if (prerequisiteBuildings.Count == 0) return true;
            var buildings = _unitOfRepository.BuildingRepository.GetBuildings(villageId);
            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var building = buildings.FirstOrDefault(x => x.Type == prerequisiteBuilding.Type);
                if (building is null) return false;
                if (building.Level < prerequisiteBuilding.Level) return false;
            }
            return true;
        }
    }
}