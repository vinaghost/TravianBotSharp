namespace MainCore.Repositories
{
    public interface IAccountInfoRepository
    {
        bool IsPlusActive(AccountId accountId);
    }
}