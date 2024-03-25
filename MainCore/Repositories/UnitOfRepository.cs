using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Repositories
{
    [RegisterAsTransient(withoutInterface: true)]
    public record UnitOfRepository(IAccountInfoRepository AccountInfoRepository,
                                   IAccountRepository AccountRepository,
                                   IAccountSettingRepository AccountSettingRepository,
                                   IBuildingRepository BuildingRepository,
                                   IFarmRepository FarmRepository,
                                   IHeroRepository HeroRepository,
                                   IHeroItemRepository HeroItemRepository,
                                   IAdventureRepository AdventureRepository,
                                   IJobRepository JobRepository,
                                   IQueueBuildingRepository QueueBuildingRepository,
                                   IStorageRepository StorageRepository,
                                   IVillageRepository VillageRepository,
                                   IExpansionSlotRepository ExpansionSlotRepository,
                                   IVillageSettingRepository VillageSettingRepository);
}