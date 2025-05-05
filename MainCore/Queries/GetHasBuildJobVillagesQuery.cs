using MainCore.Queries.Base;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetHasBuildJobVillagesQuery
    {
        public sealed record Query(AccountId AccountId) : IQuery;

        private static readonly List<JobTypeEnums> _jobTypes = new() {
                JobTypeEnums.NormalBuild,
                JobTypeEnums.ResourceBuild
            };

        private static async ValueTask<List<VillageId>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var accountId = query.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();
            var items = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Jobs.Any(x => _jobTypes.Contains(x.Type)))
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .ToList();
            return items;
        }
    }
}