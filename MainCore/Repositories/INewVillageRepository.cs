using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface INewVillageRepository
    {
        void Add(AccountId accountId, int x, int y);

        void Delete(int id);

        NewVillage Get(AccountId accountId);

        List<NewVillage> GetAll(AccountId accountId);
        bool IsExist(AccountId accountId, int x, int y);
        void Reset(AccountId accountId, int x, int y);
        void SetVillage(int id, VillageId villageId);
    }
}