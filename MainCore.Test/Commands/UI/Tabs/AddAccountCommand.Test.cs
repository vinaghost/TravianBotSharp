using FluentValidation;
using FluentValidation.Results;
using MainCore.DTO;
using MainCore.Infrasturecture.Persistence;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Test.Commands.UI.Tabs
{
    public class AddAccountCommand : Base
    {
        private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel = Substitute.For<IWaitingOverlayViewModel>();
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        private readonly IValidator<AccountInput> _accountInputValidator = Substitute.For<IValidator<AccountInput>>();
        private readonly IUseragentManager _useragentManager = Substitute.For<IUseragentManager>();
        private readonly MainCore.Commands.UI.AddAccountCommand _addAccountCommand;

        public AddAccountCommand() : base()
        {
            _accountInputValidator.ValidateAsync(Arg.Any<AccountInput>(), Arg.Any<CancellationToken>()).Returns(new ValidationResult());
            _addAccountCommand = new(_dialogService, _waitingOverlayViewModel, _mediator, _accountInputValidator, _contextFactory, _useragentManager);
        }

        [Fact]
        public async Task AddAccountCommand_WhenAccountInputIsValid_ShouldAddAccount()
        {
            // Arrange
            var internet = new Bogus.DataSets.Internet();
            var accountInput = new AccountInput
            {
                Server = internet.Url(),
                Username = internet.UserName(),
            };

            accountInput.SetAccesses([new AccessInput
            {
                Username = internet.UserName(),
                Password = internet.Password(),
            }]);

            // Act
            await _addAccountCommand.Execute(accountInput, CancellationToken.None);

            // Assert
            using var context = _contextFactory.CreateDbContext();
            var actual = context.Accounts.Any(x => x.Username == accountInput.Username);
            actual.Should().BeTrue();
        }

        [Fact]
        public async Task AddAccountCommand_WhenAccountInputIsDuplicate_ShouldNotAddAccount()
        {
            // Arrange
            var internet = new Bogus.DataSets.Internet();
            var accountInput = new AccountInput
            {
                Server = internet.Url(),
                Username = internet.UserName(),
            };

            accountInput.SetAccesses([new AccessInput
            {
                Username = internet.UserName(),
                Password = internet.Password(),
            }]);

            // Act
            await _addAccountCommand.Execute(accountInput, CancellationToken.None);
            await _addAccountCommand.Execute(accountInput, CancellationToken.None);

            // Assert
            using var context = _contextFactory.CreateDbContext();
            var actual = context.Accounts.Count();
            actual.Should().Be(1);
        }

        [Fact]
        public async Task AddAccountCommand_WhenAccountDetailsIsValid_ShouldAddAccounts()
        {
            // Arrange
            var internet = new Bogus.DataSets.Internet();
            var accountDetails = new List<AccountDetailDto>
            {
                new() {
                    Server = internet.Url(),
                    Username = internet.UserName(),
                }
            };

            // Act
            await _addAccountCommand.Execute(accountDetails, CancellationToken.None);

            // Assert
            using var context = _contextFactory.CreateDbContext();
            var actual = context.Accounts.Count();
            actual.Should().Be(1);
        }

        [Fact]
        public async Task AddAccountCommand_WhenAccountDetailsHasDuplicate_ShouldNotAddAccounts()
        {
            // Arrange
            var internet = new Bogus.DataSets.Internet();
            var accountDetails = new List<AccountDetailDto>
            {
                new() {
                    Server = internet.Url(),
                    Username = internet.UserName(),
                },
                new() {
                    Server = internet.Url(),
                    Username = internet.UserName(),
                }
            };

            // Act
            await _addAccountCommand.Execute(accountDetails, CancellationToken.None);
            await _addAccountCommand.Execute(accountDetails, CancellationToken.None);

            // Assert
            using var context = _contextFactory.CreateDbContext();
            var actual = context.Accounts.Count();
            actual.Should().Be(2);
        }

        // test for correct add account with its child
        [Fact]
        public async Task AddAccountCommand_WhenAccountAdded_ShouldAddOtherChildren()
        {
            // Arrange
            var internet = new Bogus.DataSets.Internet();
            var accountInput = new AccountInput
            {
                Server = internet.Url(),
                Username = internet.UserName(),
            };

            accountInput.SetAccesses([new AccessInput
            {
                Username = internet.UserName(),
                Password = internet.Password(),
            }]);

            // Act
            await _addAccountCommand.Execute(accountInput, CancellationToken.None);

            // Assert
            using var context = _contextFactory.CreateDbContext();
            var account = context.Accounts.FirstOrDefault();
            account.Should().NotBeNull();
            if (account is null) return;
            var id = account.Id;
            var access = context.Accesses.Count(x => x.AccountId == id);
            access.Should().Be(1);
            var info = context.AccountsInfo.Count(x => x.AccountId == id);
            info.Should().Be(1);
            var setting = context.AccountsSetting.Count(x => x.AccountId == id);
            setting.Should().Be(AppDbContext.AccountDefaultSettings.Count);
        }
    }
}