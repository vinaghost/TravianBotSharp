namespace MainCore.Repositories
{
    public interface IVillageSettingRepository
    {
        Dictionary<VillageSettingEnums, int> Get(VillageId villageId);

        bool GetBooleanByName(VillageId villageId, VillageSettingEnums setting);

        int GetByName(VillageId villageId, VillageSettingEnums setting);

        int GetByName(VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1);

        Dictionary<VillageSettingEnums, int> GetByName(VillageId villageId, List<VillageSettingEnums> settings);

        void Update(VillageId villageId, Dictionary<VillageSettingEnums, int> settings);
    }
}