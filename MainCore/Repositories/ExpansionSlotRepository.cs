using MainCore.Common.Enums;
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

        public bool IsDefaultExpansionSlot(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var expansionSlot = context.ExpansionSlots
                .Where(x => x.VillageId == villageId.Value)
                .Any();
            return !expansionSlot;
        }

        public bool IsSlotAvailable(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var settleBuildings = new List<BuildingEnums>() { BuildingEnums.Residence, BuildingEnums.Palace, BuildingEnums.CommandCenter };

            if (!context.ExpansionSlots
               .Where(x => x.VillageId == villageId.Value)
               .Any(x => x.Status != ExpansionStatusEnum.UsedExpansionSlot)) return false;

            var dto = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => settleBuildings.Contains(x.Type))
                .ToDto()
                .FirstOrDefault();
            if (dto is null) return false;

            var expansionSlots = context.ExpansionSlots
               .Where(x => x.VillageId == villageId.Value)
               .Select(x => x.Status)
               .ToList();

            if (dto.Level == 20)
            {
                if (expansionSlots.Any(x => x == ExpansionStatusEnum.FreeExpansionSlot)) return true;
                return false;
            }

            if (dto.Level == 15 && dto.Type == BuildingEnums.Palace)
            {
                // 1 locked slot for level 20
                // so 1 + 1 = 2
                if (expansionSlots.Count(x => x == ExpansionStatusEnum.FreeExpansionSlot) >= 2) return true;
                return false;
            }

            if (dto.Level == 10)
            {
                // don't need check palace because third slot is nextExpansionSlot not freeExpansionSlot
                if (expansionSlots.Count(x => x == ExpansionStatusEnum.FreeExpansionSlot) >= 2) return true;
                return false;
            }
            return false;
        }
    }
}