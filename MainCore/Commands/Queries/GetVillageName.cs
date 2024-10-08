using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetVillageName>]
    public class GetVillageName(IDbContextFactory<AppDbContext> contextFactory) : QueryBase(contextFactory)
    {
        public string Execute(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var villageName = context.Villages
                .Where(x => x.Id == villageId.Value)
                .Select(x => x.Name)
                .FirstOrDefault();
            return villageName ?? "Unknow village";
        }
    }
}