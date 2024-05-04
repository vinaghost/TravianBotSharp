using MainCore.Common.Errors.AutoBuilder;
using MainCore.DTO;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class GetJobCommand : ByAccountVillageIdBase, ICommand<JobDto>
    {
        public GetJobCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class GetJobCommandHandler : ICommandHandler<GetJobCommand, JobDto>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IAccountInfoRepository _accountInfoRepository;
        private readonly IVillageSettingRepository _villageSettingRepository;

        public GetJobCommandHandler(IJobRepository jobRepository, IBuildingRepository buildingRepository, IAccountInfoRepository accountInfoRepository, IVillageSettingRepository villageSettingRepository)
        {
            _jobRepository = jobRepository;
            _buildingRepository = buildingRepository;
            _accountInfoRepository = accountInfoRepository;
            _villageSettingRepository = villageSettingRepository;
        }

        public async Task<Result<JobDto>> Handle(GetJobCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            await Task.CompletedTask;
            return GetBuildingJob(accountId, villageId);
        }

        private Result<JobDto> GetBuildingJob(AccountId accountId, VillageId villageId)
        {
            var countJob = _jobRepository.CountBuildingJob(villageId);
            if (countJob == 0) return Skip.AutoBuilderJobQueueEmpty;

            var countQueueBuilding = _buildingRepository.CountQueueBuilding(villageId);
            if (countQueueBuilding == 0) return GetBuildingJob(villageId);

            var plusActive = _accountInfoRepository.IsPlusActive(accountId);
            var applyRomanQueueLogic = new GetVillageSetting().GetBooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (countQueueBuilding == 1)
            {
                if (plusActive) return GetBuildingJob(villageId);
                if (applyRomanQueueLogic) return GetBuildingJob(villageId, true);
                return BuildingQueue.Full;
            }

            if (countQueueBuilding == 2)
            {
                if (plusActive && applyRomanQueueLogic) return GetBuildingJob(villageId, true);
                return BuildingQueue.Full;
            }

            return BuildingQueue.Full;
        }

        private JobDto GetBuildingJob(VillageId villageId, bool romanLogic = false)
        {
            var job = romanLogic ? GetJobBasedOnRomanLogic(villageId) : _jobRepository.GetBuildingJob(villageId);
            return job;
        }

        private JobDto GetJobBasedOnRomanLogic(VillageId villageId)
        {
            var countQueueBuilding = _buildingRepository.CountQueueBuilding(villageId);
            var countResourceQueueBuilding = _buildingRepository.CountResourceQueueBuilding(villageId);
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;
            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                return _jobRepository.GetInfrastructureBuildingJob(villageId);
            }
            else
            {
                return _jobRepository.GetResourceBuildingJob(villageId);
            }
        }
    }
}