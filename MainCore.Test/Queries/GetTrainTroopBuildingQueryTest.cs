using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetTrainTroopBuildingQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext();
            var handleBehavior = new GetTrainTroopBuildingQuery.HandleBehavior(context);
            var villageId = new VillageId(1);
            var query = new GetTrainTroopBuildingQuery.Query(villageId);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}