using FluentAssertions;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class HeroItemRepositoryTest : RepositoryTestBase<HeroItemRepository>
    {
        [TestMethod]
        public void IsEnoughResource_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.IsEnoughResource(Constants.AccountId, new long[4] { 1, 2, 3, 4 });
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Update_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Update(Constants.AccountId, new());
            func.Should().NotThrow();
        }
    }
}