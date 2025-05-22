using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetActiveFarmsQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext();
            var handleBehavior = new GetActiveFarmsQuery.HandleBehavior(context);
            var accountId = new AccountId(3041975);
            var query = new GetActiveFarmsQuery.Query(accountId);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}