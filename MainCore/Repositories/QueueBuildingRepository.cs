namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class QueueBuildingRepository : IQueueBuildingRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public QueueBuildingRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public bool IsSkippableBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var buildings = new List<BuildingEnums>()
            {
                BuildingEnums.Site,
                BuildingEnums.Residence,
                BuildingEnums.Palace,
                BuildingEnums.CommandCenter,
            };

            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => !buildings.Contains(x.Type))
                .Any();
            return queueBuilding;
        }

        public void Clean(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var now = DateTime.Now;
            var completeBuildingQuery = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Where(x => x.CompleteTime < now);

            var completeBuildingLocations = completeBuildingQuery
                .Select(x => x.Location)
                .ToList();

            foreach (var completeBuildingLocation in completeBuildingLocations)
            {
                context.Buildings
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Location == completeBuildingLocation)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Level, x => x.Level + 1));
            }

            completeBuildingQuery
                .ExecuteUpdate(x => x.SetProperty(x => x.Type, BuildingEnums.Site));
        }

        public int Count(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }
    }
}