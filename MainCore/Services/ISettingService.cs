namespace MainCore.Services
{
    public interface ISettingService
    {
        bool BooleanByName(AccountId accountId, AccountSettingEnums setting);

        bool BooleanByName(VillageId villageId, VillageSettingEnums setting);

        int ByName(AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax, int multiplier = 1);

        int ByName(AccountId accountId, AccountSettingEnums setting);

        Dictionary<VillageSettingEnums, int> ByName(VillageId villageId, List<VillageSettingEnums> settings);

        int ByName(VillageId villageId, VillageSettingEnums setting);

        int ByName(VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1);
    }
}
