using MainCore.Entities;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetAccessesQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext();
            var handleBehavior = new GetAccessesQuery.HandleBehavior(context);

            var accountId = new AccountId(3041975);
            var query = new GetAccessesQuery.Query(accountId);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}