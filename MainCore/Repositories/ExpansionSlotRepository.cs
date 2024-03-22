using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class ExpansionSlotRepository : IExpansionSlotRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ExpansionSlotRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Update(VillageId villageId, List<ExpansionSlotDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var expansionSlot = context.ExpansionSlots
                .Where(x => x.VillageId == villageId.Value)
                .ExecuteDelete();

            dtos.ForEach(x =>
            {
                context.Add(x.ToEntity(villageId));
            });

            context.SaveChanges();
        }
    }
}