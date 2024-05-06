namespace MainCore.Repositories
{
    public interface IAccountSettingRepository
    {
        Dictionary<AccountSettingEnums, int> Get(AccountId accountId);

        void Update(AccountId accountId, Dictionary<AccountSettingEnums, int> settings);
    }
}