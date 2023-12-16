using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class BuildingRepositoryTest : RepositoryTestBase<BuildingRepository>
    {
        [TestMethod]
        public void CountQueueBuilding_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.CountQueueBuilding(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void CountResourceQueueBuilding_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.CountResourceQueueBuilding(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetBuilding_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetBuilding(Constants.VillageId, 1);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetNormalBuilding_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetNormalBuilding(Constants.VillageId, Constants.BuildingId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetItems_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetItems(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetCropland_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetCropland(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetNormalBuildPlan_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetNormalBuildPlan(Constants.VillageId, new ResourceBuildPlan());
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetBuildingLocation_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetBuildingLocation(Constants.VillageId, BuildingEnums.Academy);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void IsEmptySite_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.EmptySite(Constants.VillageId, 2);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void IsRallyPointExists_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.IsRallyPointExists(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetLevelBuildings_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetBuildingItems(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetTrainTroopBuilding_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetTrainTroopBuilding(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Update_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Update(Constants.VillageId, new List<BuildingDto>());
            func.Should().NotThrow();
        }
    }
}