using MainCore.Constraints;

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
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

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