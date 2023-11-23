using FakeItEasy;
using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using TestProject.Fake;

namespace TestProject.Repositories
{
    [TestClass]
    public class AccountRepositoryTest : RepositoryTestBase<AccountRepository>
    {
        protected override AccountRepository GetRepository()
        {
            var contextFactory = new FakeDbContextFactory();
            contextFactory.Setup();
            var useragentManager = A.Fake<IUseragentManager>();
            A.CallTo(() => useragentManager.Get()).Returns("empty useragent");
            var taskManager = A.Fake<ITaskManager>();
            A.CallTo(() => taskManager.GetStatus(A<AccountId>._)).Returns(StatusEnums.Offline);
            var repository = new AccountRepository(contextFactory, useragentManager, taskManager);
            return repository;
        }

        [TestMethod]
        public void Add_Single_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Add(new AccountDto()
            {
                Accesses = new List<AccessDto>(),
            });
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Add_Multiple_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Add(new List<AccountDetailDto>() {
                new AccountDetailDto(),
                new AccountDetailDto(),
            });
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Delete_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Delete(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Get_IncludeAccess_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Get(Constants.AccountId, true);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Get_NotIncludeAccess_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Get(Constants.AccountId, false);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetAccess_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetAccess(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetAccesses_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetAccesses(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetItems_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetItems();
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetUsername_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetUsername(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void GetPassword_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.GetPassword(Constants.AccountId);
            func.Should().NotThrow();
        }

        [TestMethod]
        public void Update_ShouldNotThrow()
        {
            var repository = GetRepository();
            var func = () => repository.Update(new AccountDto()
            {
                Id = Constants.AccountId,
                Accesses = new List<AccessDto>(),
            });
            func.Should().NotThrow();
        }
    }
}