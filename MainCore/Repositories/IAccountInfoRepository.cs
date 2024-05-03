using MainCore.DTO;

namespace MainCore.Repositories
{
    public interface IAccountInfoRepository
    {
        bool IsPlusActive(AccountId accountId);
        void Update(AccountId accountId, AccountInfoDto dto);
    }
}