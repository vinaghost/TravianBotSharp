using MainCore.UI.Models.Output;

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
                .Where(x => x.Id == villageId.Value)
                .Select(x => x.Name)
                .FirstOrDefault();
            return villageName;
        }

        public VillageId GetActiveVillages(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
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
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => !x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .ToList();
            return villages;
        }

        public List<VillageId> Get(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var villages = context.Villages
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
    }
}