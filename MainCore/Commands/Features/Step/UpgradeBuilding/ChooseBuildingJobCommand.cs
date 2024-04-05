using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

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
                    return Result.Fail(Skip.AutoBuilderJobQueueEmpty);
                }

                var countQueueBuilding = _unitOfRepository.BuildingRepository.CountQueueBuilding(command.VillageId);

                if (countQueueBuilding == 0)
                {
                    var job = _unitOfRepository.JobRepository.GetBuildingJob(command.VillageId);
                    if (await JobComplete(command.AccountId, command.VillageId, job, cancellationToken)) continue;
                    var result = _unitOfRepository.JobRepository.JobValid(command.VillageId, job);
                    if (result.IsFailed)
                    {
                        return result
                            .WithError(new Stop("order building in queue building is not correct, please check"));
                    }
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
                        if (await JobComplete(command.AccountId, command.VillageId, job, cancellationToken)) continue;
                        var result = _unitOfRepository.JobRepository.JobValid(command.VillageId, job);
                        if (result.IsFailed)
                        {
                            job = _unitOfRepository.JobRepository.GetResourceBuildingJob(command.VillageId);
                            if (job is null) return result;
                        }
                        Value = job;
                        return Result.Ok();
                    }
                    if (applyRomanQueueLogic)
                    {
                        var job = GetJobBasedOnRomanLogic(command.VillageId, countQueueBuilding);
                        if (job is null) return Result.Fail(BuildingQueue.NotTaskInqueue);
                        if (await JobComplete(command.AccountId, command.VillageId, job, cancellationToken)) continue;
                        var result = _unitOfRepository.JobRepository.JobValid(command.VillageId, job);
                        if (result.IsFailed)
                        {
                            return result;
                        }
                        Value = job;
                        return Result.Ok();
                    }
                    return Result.Fail(BuildingQueue.Full);
                }

                if (countQueueBuilding == 2)
                {
                    if (plusActive && applyRomanQueueLogic)
                    {
                        var job = GetJobBasedOnRomanLogic(command.VillageId, countQueueBuilding);
                        if (job is null) return Result.Fail(BuildingQueue.NotTaskInqueue);
                        if (await JobComplete(command.AccountId, command.VillageId, job, cancellationToken)) continue;
                        var result = _unitOfRepository.JobRepository.JobValid(command.VillageId, job);
                        if (result.IsFailed)
                        {
                            return result;
                        }
                        Value = job;
                        return Result.Ok();
                    }
                    return Result.Fail(BuildingQueue.Full);
                }
                return Result.Fail(BuildingQueue.Full);
            }
            while (true);
        }

        private async Task<bool> JobComplete(AccountId accountId, VillageId villageId, JobDto job, CancellationToken cancellationToken)
        {
            if (_unitOfRepository.JobRepository.JobComplete(villageId, job))
            {
                _unitOfRepository.JobRepository.Delete(job.Id);
                await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
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
    }
}