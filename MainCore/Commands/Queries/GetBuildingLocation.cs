namespace MainCore.Commands.Queries
{
    public class GetBuildingLocation(IDbContextFactory<AppDbContext> contextFactory = null)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();

        public int Execute(VillageId villageId, BuildingEnums building)
        {
            using var context = _contextFactory.CreateDbContext();
            var location = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == building)
                .Select(x => x.Location)
                .FirstOrDefault();
            return location;
        }
    }
}