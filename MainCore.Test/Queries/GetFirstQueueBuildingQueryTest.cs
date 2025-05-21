using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetFirstQueueBuildingQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext(true);
            context.Add(new QueueBuilding()
            {
                VillageId = 1,
                Location = 5,
                Type = Enums.BuildingEnums.Workshop,
                Position = 12,
            });
            context.SaveChanges();
            var handleBehavior = new GetFirstQueueBuildingQuery.HandleBehavior(context);
            var query = new GetFirstQueueBuildingQuery.Query(new VillageId(1));
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}