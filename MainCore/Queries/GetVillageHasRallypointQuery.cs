using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetVillageHasRallypointQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<VillageId> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;
            var villageHasRallypoint = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Any(x => x.Type == BuildingEnums.RallyPoint && x.Level > 0))
                .OrderByDescending(x => x.IsActive)
                .Select(x => new VillageId(x.Id))
                .FirstOrDefault();
            return villageHasRallypoint;
        }
    }
}