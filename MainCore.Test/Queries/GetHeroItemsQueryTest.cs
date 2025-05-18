using MainCore.Entities;
using MainCore.Enums;
using MainCore.Queries;

namespace MainCore.Test.Queries
{
    public class GetHeroItemsQueryTest
    {
        [Fact]
        public async Task QueryShouldRun()
        {
            using var context = new FakeDbContextFactory().CreateDbContext();
            var handleBehavior = new GetHeroItemsQuery.HandleBehavior(context);
            var accountId = new AccountId(1);
            var query = new GetHeroItemsQuery.Query(accountId, [HeroItemEnums.ArmorBreastplate1]);
            await Should.NotThrowAsync(async () => await handleBehavior.HandleAsync(query, CancellationToken.None));
        }
    }
}