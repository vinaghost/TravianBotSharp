using MainCore.Entities;
using MainCore.Enums;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetLowestBuildingQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext(true);
            context.Add(new Building
            {
                VillageId = 1,
                Type = BuildingEnums.MainBuilding,
                Level = 1,
            });
            context.SaveChanges();

            var handleBehavior = new GetLowestBuildingQuery.HandleBehavior(context);
            var villageId = new VillageId(1);
            var buildingType = BuildingEnums.MainBuilding;
            var query = new GetLowestBuildingQuery.Query(villageId, buildingType);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}