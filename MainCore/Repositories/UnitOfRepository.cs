namespace MainCore.Repositories
{
    [RegisterAsTransient(withoutInterface: true)]
    public record UnitOfRepository(IAccountInfoRepository AccountInfoRepository,
                                   IAccountRepository AccountRepository,
                                   IAccountSettingRepository AccountSettingRepository,
                                   IBuildingRepository BuildingRepository,
                                   IFarmRepository FarmRepository,
                                   IHeroItemRepository HeroItemRepository,
                                   IJobRepository JobRepository,
                                   IQueueBuildingRepository QueueBuildingRepository,
                                   IStorageRepository StorageRepository,
                                   IVillageRepository VillageRepository,
                                   IVillageSettingRepository VillageSettingRepository);
}