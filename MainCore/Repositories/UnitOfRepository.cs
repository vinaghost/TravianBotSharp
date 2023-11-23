using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class UnitOfRepository : IUnitOfRepository
    {
        public UnitOfRepository(IAccountInfoRepository accountInfoRepository, IAccountRepository accountRepository, IAccountSettingRepository accountSettingRepository, IBuildingRepository buildingRepository, IFarmRepository farmRepository, IHeroItemRepository heroItemRepository, IJobRepository jobRepository, IQueueBuildingRepository queueBuildingRepository, IStorageRepository storageRepository, IVillageRepository villageRepository, IVillageSettingRepository villageSettingRepository)
        {
            AccountInfoRepository = accountInfoRepository;
            AccountRepository = accountRepository;
            AccountSettingRepository = accountSettingRepository;
            BuildingRepository = buildingRepository;
            FarmRepository = farmRepository;
            HeroItemRepository = heroItemRepository;
            JobRepository = jobRepository;
            QueueBuildingRepository = queueBuildingRepository;
            StorageRepository = storageRepository;
            VillageRepository = villageRepository;
            VillageSettingRepository = villageSettingRepository;
        }

        public IAccountInfoRepository AccountInfoRepository { get; }
        public IAccountRepository AccountRepository { get; }
        public IAccountSettingRepository AccountSettingRepository { get; }
        public IBuildingRepository BuildingRepository { get; }
        public IFarmRepository FarmRepository { get; }
        public IHeroItemRepository HeroItemRepository { get; }
        public IJobRepository JobRepository { get; }
        public IQueueBuildingRepository QueueBuildingRepository { get; }
        public IStorageRepository StorageRepository { get; }
        public IVillageRepository VillageRepository { get; }
        public IVillageSettingRepository VillageSettingRepository { get; }
    }
}