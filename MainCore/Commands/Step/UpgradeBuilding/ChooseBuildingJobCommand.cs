using FluentResults;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    [RegisterAsTransient]
    public class ChooseBuildingJobCommand : IChooseBuildingJobCommand
    {
        private readonly IUnitOfRepository _unitOfRepository;

        private readonly IMediator _mediator;

        public ChooseBuildingJobCommand(IUnitOfRepository unitOfRepository, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public JobDto Value { get; private set; }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId)
        {
            do
            {
                var countJob = _unitOfRepository.JobRepository.CountBuildingJob(villageId);

                if (countJob == 0)
                {
                    return Result.Fail(Skip.BuildingJobQueueEmpty);
                }

                var countQueueBuilding = _unitOfRepository.BuildingRepository.CountQueueBuilding(villageId);

                if (countQueueBuilding == 0)
                {
                    var job = _unitOfRepository.JobRepository.GetBuildingJob(villageId);
                    if (await JobComplete(accountId, villageId, job)) continue;
                    Value = job;
                    return Result.Ok();
                }

                var plusActive = _unitOfRepository.AccountInfoRepository.IsPlusActive(accountId);
                var applyRomanQueueLogic = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

                if (countQueueBuilding == 1)
                {
                    if (plusActive)
                    {
                        var job = _unitOfRepository.JobRepository.GetBuildingJob(villageId);
                        if (await JobComplete(accountId, villageId, job)) continue;
                        Value = job;
                        return Result.Ok();
                    }
                    if (applyRomanQueueLogic)
                    {
                        var job = GetJobBasedOnRomanLogic(villageId, countQueueBuilding);
                        if (job is null) return Result.Fail(BuildingQueue.NotTaskInqueue);
                        if (await JobComplete(accountId, villageId, job)) continue;
                        Value = job;
                        return Result.Ok();
                    }
                    return Result.Fail(BuildingQueue.Full);
                }

                if (countQueueBuilding == 2)
                {
                    if (applyRomanQueueLogic)
                    {
                        var job = GetJobBasedOnRomanLogic(villageId, countQueueBuilding);
                        if (job is null) return Result.Fail(BuildingQueue.NotTaskInqueue);
                        if (await JobComplete(accountId, villageId, job)) continue;
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
    }
}