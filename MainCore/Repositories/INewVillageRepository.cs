using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface INewVillageRepository
    {
        NewVillage Get(AccountId accountId);
    }
}