using FluentAssertions;
using MainCore.DTO;
using MainCore.Repositories;

namespace TestProject.Repositories
{
    [TestClass]
    public class AccountInfoRepositoryTest : RepositoryTestBase<AccountInfoRepository>
    {
        [TestMethod]
        public void IsPlusActive_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.IsPlusActive(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Update_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Update(Constants.AccountId, new AccountInfoDto());
            func.Should().NotThrow();
        }
    }
}