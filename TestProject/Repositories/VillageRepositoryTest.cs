using FluentAssertions;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class VillageRepositoryTest : RepositoryTestBase<VillageRepository>
    {
        [TestMethod]
        public void Get_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Get(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetActiveVillages_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetActiveVillages(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetHasBuildingJobVillages_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetHasBuildingJobVillages(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetInactiveVillages_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetInactiveVillages(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetMissingBuildingVillages_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetMissingBuildingVillages(Constants.AccountId);
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
        public void GetVillageName_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetVillageName(Constants.VillageId);
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