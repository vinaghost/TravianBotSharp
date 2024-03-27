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

        public bool IsExist(AccountId accountId, int x, int y)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.NewVillages
                .Where(x => x.AccountId == accountId.Value)
                .Any(v => v.X == x && v.Y == y);
        }

        public void Reset(AccountId accountId, int x, int y)
        {
            using var context = _contextFactory.CreateDbContext();
            context.NewVillages
                .Where(x => x.AccountId == accountId.Value)
                .Where(v => v.X == x && v.Y == y)
                .ExecuteUpdate(x => x.SetProperty(x => x.VillageId, x => 0));
        }

        public void Delete(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var coordinates = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new { x.X, x.Y })
                .ToList();

            var newVillages = context.NewVillages
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            foreach (var newVillage in newVillages)
            {
                if (coordinates.Any(x => x.X == newVillage.X && x.Y == newVillage.Y)) context.Remove(newVillage);
            }

            context.SaveChanges();
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

        public bool IsSettling(AccountId accountId, VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            return context.NewVillages
                .Where(x => x.AccountId == accountId.Value)
                .Any(x => x.VillageId == villageId.Value);
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