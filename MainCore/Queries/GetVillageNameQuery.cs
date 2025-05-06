using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetVillageNameQuery
    {
        public sealed record Query(VillageId VillageId) : IQuery;

        private static async ValueTask<string> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var villageId = query.VillageId;

            var villageName = context.GetVillageName(villageId);
            return villageName;
        }

        public static string GetVillageName(this AppDbContext context, VillageId villageId)
        {
            var villageName = context.Villages
                .Where(x => x.Id == villageId.Value)
                .Select(x => x.Name)
                .FirstOrDefault();
            return villageName ?? "Unknow village";
        }
    }
}