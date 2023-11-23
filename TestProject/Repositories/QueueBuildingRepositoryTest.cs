using FluentAssertions;
using MainCore.DTO;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class QueueBuildingRepositoryTest : RepositoryTestBase<QueueBuildingRepository>
    {
        [TestMethod]
        public void GetFirst_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetFirst(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Clean_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Clean(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Count_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Count(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Update_QueueBuilding_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Update(Constants.VillageId, new List<BuildingDto>());
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Update_BuildingDto_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Update(Constants.VillageId, new List<QueueBuildingDto>());
            func.Should().NotThrow();
        }
    }
}