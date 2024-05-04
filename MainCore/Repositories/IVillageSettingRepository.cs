namespace MainCore.Repositories
{
    public interface IVillageSettingRepository
    {
        Dictionary<VillageSettingEnums, int> Get(VillageId villageId);

        void Update(VillageId villageId, Dictionary<VillageSettingEnums, int> settings);
    }
}