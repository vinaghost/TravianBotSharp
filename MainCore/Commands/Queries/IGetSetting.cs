
namespace MainCore.Commands.Queries
{
    public interface IGetSetting
    {
        bool BooleanByName(AccountId accountId, AccountSettingEnums setting);
        bool BooleanByName(VillageId villageId, VillageSettingEnums setting);
        int ByName(AccountId accountId, AccountSettingEnums setting);
        int ByName(AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax, int multiplier = 1);
        Dictionary<VillageSettingEnums, int> ByName(VillageId villageId, List<VillageSettingEnums> settings);
        int ByName(VillageId villageId, VillageSettingEnums setting);
        int ByName(VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1);
        Dictionary<AccountSettingEnums, int> Get(AccountId accountId);
        Dictionary<VillageSettingEnums, int> Get(VillageId villageId);
    }
}