using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetHasBuildJobVillagesQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext(true);
            var handleBehavior = new GetHasBuildJobVillagesQuery.HandleBehavior(context);
            var accountId = new AccountId(1);
            var query = new GetHasBuildJobVillagesQuery.Query(accountId);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}