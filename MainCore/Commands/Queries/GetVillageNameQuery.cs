using MainCore.Commands.Base;

namespace MainCore.Commands.Queries
{
    [Handler]
    public static partial class GetVillageNameQuery
    {
        public sealed record Query(VillageId VillageId) : IQuery;

        private static async ValueTask<string> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var villageId = query.VillageId;
            using var context = await contextFactory.CreateDbContextAsync();

            var villageName = context.Villages
                .Where(x => x.Id == villageId.Value)
                .Select(x => x.Name)
                .FirstOrDefault();
            return villageName ?? "Unknow village";
        }
    }
}