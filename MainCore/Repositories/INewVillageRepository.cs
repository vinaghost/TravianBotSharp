using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface INewVillageRepository
    {
        void Add(AccountId accountId, int x, int y);

        void Delete(int id);

        NewVillage Get(AccountId accountId);

        List<NewVillage> GetAll(AccountId accountId);
    }
}