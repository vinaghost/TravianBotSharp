using Ardalis.Specification;

namespace MainCore.Specifications
{
    public class HasBuildJobVillagesSpec : Specification<Village, VillageId>
    {
        private static readonly List<JobTypeEnums> _jobTypes = new() {
            JobTypeEnums.NormalBuild,
            JobTypeEnums.ResourceBuild
        };

        public HasBuildJobVillagesSpec(AccountId accountId)
        {
            Query
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Jobs.Any(x => _jobTypes.Contains(x.Type)))
                .Select(x => new VillageId(x.Id));
        }
    }
}