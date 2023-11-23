using FluentAssertions;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class FarmRepositoryTest : RepositoryTestBase<FarmRepository>
    {
        [TestMethod]
        public void GetActive_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetActive(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void ChangeActive_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.ChangeActive(Constants.FarmId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void CountActive_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.CountActive(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetItems_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetItems(Constants.AccountId);
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