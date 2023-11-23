using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class VillageSettingRepositoryTest : RepositoryTestBase<VillageSettingRepository>
    {
        [TestMethod]
        public void Get_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Get(Constants.VillageId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetBooleanByName_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetBooleanByName(Constants.VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetByName_Single_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetByName(Constants.VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetByName_Double_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetByName(Constants.VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin, VillageSettingEnums.TrainTroopRepeatTimeMax);
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