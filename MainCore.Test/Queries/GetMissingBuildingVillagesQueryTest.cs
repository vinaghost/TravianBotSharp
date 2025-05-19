using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetMissingBuildingVillagesQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext();

            var handleBehavior = new GetMissingBuildingVillagesQuery.HandleBehavior(context);
            var accountId = new AccountId(1);
            var query = new GetMissingBuildingVillagesQuery.Query(accountId);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}