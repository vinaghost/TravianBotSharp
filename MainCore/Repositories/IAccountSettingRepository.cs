using MainCore.Common.Enums;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IAccountSettingRepository
    {
        Dictionary<AccountSettingEnums, int> Get(AccountId accountId);
        bool GetBooleanByName(AccountId accountId, AccountSettingEnums setting);

        int GetByName(AccountId accountId, AccountSettingEnums setting);

        int GetByName(AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax);
        void Update(AccountId accountId, Dictionary<AccountSettingEnums, int> settings);
    }
}