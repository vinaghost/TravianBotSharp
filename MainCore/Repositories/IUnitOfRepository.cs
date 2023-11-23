namespace MainCore.Repositories
{
    public interface IUnitOfRepository
    {
        IAccountInfoRepository AccountInfoRepository { get; }
        IAccountRepository AccountRepository { get; }
        IAccountSettingRepository AccountSettingRepository { get; }
        IBuildingRepository BuildingRepository { get; }
        IFarmRepository FarmRepository { get; }
        IHeroItemRepository HeroItemRepository { get; }
        IJobRepository JobRepository { get; }
        IQueueBuildingRepository QueueBuildingRepository { get; }
        IStorageRepository StorageRepository { get; }
        IVillageRepository VillageRepository { get; }
        IVillageSettingRepository VillageSettingRepository { get; }
    }
}