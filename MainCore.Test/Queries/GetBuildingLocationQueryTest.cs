using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetBuildingLocationQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext(true);

            context.Add(new Building()
            {
                VillageId = 1,
                Location = 5,
                Type = Enums.BuildingEnums.Workshop
            });
            context.SaveChanges();

            var handleBehavior = new GetBuildingLocationQuery.HandleBehavior(context);
            var query = new GetBuildingLocationQuery.Query(new VillageId(1), Enums.BuildingEnums.Workshop);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}