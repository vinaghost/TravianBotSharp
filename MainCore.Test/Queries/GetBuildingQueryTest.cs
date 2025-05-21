using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetBuildingQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext(true);

            context.Add(new Building
            {
                Id = 1,
                VillageId = 1,
                Location = 5,
                Level = 1,
            });
            context.SaveChanges();

            var handleBehavior = new GetBuildingQuery.HandleBehavior(context);
            var query = new GetBuildingQuery.Query(new VillageId(1), 5);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}