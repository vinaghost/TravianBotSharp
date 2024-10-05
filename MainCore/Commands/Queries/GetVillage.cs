using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetVillage>]
    public class GetVillage(IDbContextFactory<AppDbContext> contextFactory) : QueryBase(contextFactory)
    {
        public List<VillageId> All(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new VillageId(x.Id))
                .ToList();
            return villages;
        }

        public List<VillageId> Missing(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var missingBuildingVillages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Count != 40)
                .Select(x => new VillageId(x.Id))
                .ToList();
            return missingBuildingVillages;
        }

        public VillageId Active(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Select(x => new VillageId(x.Id))
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
                .Select(x => new VillageId(x.Id))
                .ToList();
            return villages;
        }

        public List<VillageId> HasBuildingJob(AccountId accountId)
        {
            var types = new List<JobTypeEnums>() {
                JobTypeEnums.NormalBuild,
                JobTypeEnums.ResourceBuild
            };
            using var context = _contextFactory.CreateDbContext();
            var hasBuildingJobVillages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Jobs.Any(x => types.Contains(x.Type)))
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .ToList();
            return hasBuildingJobVillages;
        }
    }
}