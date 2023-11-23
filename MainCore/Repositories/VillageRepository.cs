using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using MainCore.UI.Models.Output;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class VillageRepository : IVillageRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public VillageRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public string GetVillageName(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var villageName = context.Villages
                .AsNoTracking()
                .Where(x => x.Id == villageId.Value)
                .Select(x => x.Name)
                .FirstOrDefault();
            return villageName;
        }

        public VillageId GetActiveVillages(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
                .AsNoTracking()
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .FirstOrDefault();
            return village;
        }

        public List<VillageId> GetInactiveVillages(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var villages = context.Villages
                .AsNoTracking()
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => !x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .ToList();
            return villages;
        }

        public VillageId GetVillageHasRallypoint(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
                .AsNoTracking()
                .Where(x => x.AccountId == accountId.Value)
                .Include(x => x.Buildings.Where(x => x.Type == BuildingEnums.RallyPoint))
                .Where(x => x.Buildings.Count > 0)
                .OrderByDescending(x => x.IsActive)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .FirstOrDefault();
            return village;
        }

        public List<VillageId> Get(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var villages = context.Villages
                .AsNoTracking()
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .ToList();
            return villages;
        }

        public List<VillageId> GetMissingBuildingVillages(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var missingBuildingVillages = context.Villages
                .AsNoTracking()
                .Where(x => x.AccountId == accountId.Value)
                .Include(x => x.Buildings)
                .Where(x => x.Buildings.Count != 40)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .ToList();
            return missingBuildingVillages;
        }

        public List<VillageId> GetHasBuildingJobVillages(AccountId accountId)
        {
            var types = new List<JobTypeEnums>() {
                JobTypeEnums.NormalBuild,
                JobTypeEnums.ResourceBuild
            };
            using var context = _contextFactory.CreateDbContext();
            var hasBuildingJobVillages = context.Villages
                .AsNoTracking()
                .Where(x => x.AccountId == accountId.Value)
                .Include(x => x.Jobs.Where(x => types.Contains(x.Type)))
                .Where(x => x.Jobs.Count() > 0)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .ToList();
            return hasBuildingJobVillages;
        }

        public List<ListBoxItem> GetItems(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var villages = context.Villages
                .AsNoTracking()
                .Where(x => x.AccountId == accountId.Value)
                .OrderBy(x => x.Name)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Content = $"{x.Name}{Environment.NewLine}({x.X}|{x.Y})",
                })
                .ToList();
            return villages;
        }

        public void Update(AccountId accountId, List<VillageDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            var ids = dtos.Select(x => x.Id.Value).ToList();

            var villageDeleted = villages.Where(x => !ids.Contains(x.Id)).ToList();
            var villageInserted = dtos.Where(x => !villages.Any(v => v.Id == x.Id.Value)).ToList();
            var villageUpdated = villages.Where(x => ids.Contains(x.Id)).ToList();

            villageDeleted.ForEach(x => context.Remove(x));
            villageInserted.ForEach(x => context.Add(x.ToEntity(accountId)));

            foreach (var village in villageUpdated)
            {
                var dto = dtos.FirstOrDefault(x => x.Id.Value == village.Id);
                dto.To(village);
                context.Update(village);
            }

            context.SaveChanges();
        }
    }
}