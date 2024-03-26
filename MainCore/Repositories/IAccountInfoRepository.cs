using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IAccountInfoRepository
    {
        string GetTemplatePath(AccountId accountId);
        bool IsEnoughCP(AccountId accountId);
        bool IsPlusActive(AccountId accountId);
        void SetTemplatePath(AccountId accountId, string path);
        void Update(AccountId accountId, AccountInfoDto dto);
    }
}