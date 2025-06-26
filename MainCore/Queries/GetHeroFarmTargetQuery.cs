using MainCore.Constraints;
using MainCore.UI.Models.Output;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetHeroFarmTargetQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<List<HeroFarmItem>> HandleAsync(
           Query query,
           AppDbContext context,
           CancellationToken cancellationToken
           )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

            var targets = context.HeroFarmTargets
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new HeroFarmItem()
                {
                    Id = x.Id,
                    X = x.X,
                    Y = x.Y,
                    Animal = x.Animal,
                    Resource = x.Resource,
                    OasisType = x.OasisType,
                    LastSend = x.LastSend
                })
                .ToList();

            return targets;
        }
    }
}