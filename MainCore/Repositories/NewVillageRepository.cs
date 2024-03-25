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

        public NewVillage Get(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var newVillage = context.NewVillages
                .Where(x => x.AccountId == accountId.Value)
                .FirstOrDefault();
            return newVillage;
        }
    }
}