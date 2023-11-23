using FluentAssertions;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Repositories;
using System.Text.Json;

namespace TestProject.Repositories
{
    [TestClass]
    public class JobRepositoryTest : RepositoryTestBase<JobRepository>
    {
        [TestMethod]
        public void Add_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Add(Constants.VillageId, new NormalBuildPlan());
            func.Should().NotThrow();
        }

        [TestMethod]
        public void AddToTop_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.AddToTop(Constants.VillageId, new NormalBuildPlan());
            func.Should().NotThrow();
        }

        [TestMethod]
        public void CountBuildingJob_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.CountBuildingJob(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Delete_Single_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Delete(Constants.JobId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Delete_All_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Delete(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetBuildingJob_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetBuildingJob(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetInfrastructureBuildingJob_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetInfrastructureBuildingJob(Constants.VillageId);
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
        public void GetResourceBuildingJob_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetResourceBuildingJob(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Move_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Move(Constants.JobId, Constants.JobId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void IsJobComplete_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.JobComplete(Constants.VillageId, new JobDto()
            {
                Content = JsonSerializer.Serialize(new NormalBuildPlan())
            });
            func.Should().NotThrow();
        }
    }
}