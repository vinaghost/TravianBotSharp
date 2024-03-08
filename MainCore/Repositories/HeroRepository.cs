using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class HeroRepository : IHeroRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public HeroRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public HeroDto Get(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Heroes
                .Where(x => x.AccountId == accountId.Value)
                .ToDto()
                .FirstOrDefault();
        }

        public void Update(AccountId accountId, HeroDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbHero = context.Heroes
                .Where(x => x.AccountId == accountId.Value)
                .FirstOrDefault();

            if (dbHero is null)
            {
                var hero = dto.ToEntity(accountId);
                context.Add(hero);
            }
            else
            {
                dto.To(dbHero);
                context.Update(dbHero);
            }
            context.SaveChanges();
        }
    }
}