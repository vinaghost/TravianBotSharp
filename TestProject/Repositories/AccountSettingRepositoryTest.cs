using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class AccountSettingRepositoryTest : RepositoryTestBase<AccountSettingRepository>
    {
        [TestMethod]
        public void Get_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Get(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetBooleanByName_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetBooleanByName(Constants.AccountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetByName_Single_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetByName(Constants.AccountId, AccountSettingEnums.ClickDelayMin);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetByName_Double_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetByName(Constants.AccountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
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