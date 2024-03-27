using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class NewVillageRepository : INewVillageRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public NewVillageRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Add(AccountId accountId, int x, int y)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Add(new NewVillage()
            {
                AccountId = accountId.Value,
                X = x,
                Y = y,
            });
            context.SaveChanges();
        }

        public void Delete(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            context.NewVillages
                .Where(x => x.Id == id)
                .ExecuteDelete();
        }

        public void SetVillage(int id, VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.NewVillages
                .Where(x => x.Id == id)
                .ExecuteUpdate(x => x.SetProperty(x => x.VillageId, x => villageId.Value));
        }

        public NewVillage Get(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var newVillage = context.NewVillages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.VillageId == 0)
                .FirstOrDefault();
            return newVillage;
        }

        public List<NewVillage> GetAll(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var newVillages = context.NewVillages
                .Where(x => x.AccountId == accountId.Value)
                .ToList();
            return newVillages;
        }
    }
}