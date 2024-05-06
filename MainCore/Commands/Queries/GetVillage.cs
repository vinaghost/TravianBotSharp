namespace MainCore.Commands.Queries
{
    public class GetVillage
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GetVillage(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public List<VillageId> All(AccountId accountId)
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

        public List<VillageId> Missing(AccountId accountId)
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

        public VillageId Active(AccountId accountId)
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

        public List<VillageId> Inactive(AccountId accountId)
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

        public List<VillageId> Job(AccountId accountId)
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
    }
}