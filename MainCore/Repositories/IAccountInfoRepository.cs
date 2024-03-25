using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IAccountInfoRepository
    {
        bool IsEnoughCP(AccountId accountId);
        bool IsPlusActive(AccountId accountId);
        void Update(AccountId accountId, AccountInfoDto dto);
    }
}