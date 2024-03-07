using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class AdventureRepository : IAdventureRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AdventureRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Update(AccountId accountId, List<AdventureDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Adventures
                .Where(x => x.AccountId == accountId.Value)
                .ExecuteDelete();

            var entities = dtos.Select(x => x.ToEntity(accountId));
            context.AddRange(entities);
            context.SaveChanges();
        }
    }
}