using FluentAssertions;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class StorageRepositoryTest : RepositoryTestBase<StorageRepository>
    {
        [TestMethod]
        public void IsEnoughResource_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.IsEnoughResource(Constants.VillageId, new long[4] { 1, 2, 3, 4 });
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetMissingResource_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetMissingResource(Constants.VillageId, new long[4] { 1, 2, 3, 4 });
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetGranaryPercent_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetGranaryPercent(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Update_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Update(Constants.VillageId, new());
            func.Should().NotThrow();
        }
    }
}