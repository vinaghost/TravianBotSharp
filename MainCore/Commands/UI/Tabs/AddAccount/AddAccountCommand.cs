using FluentValidation;
using MainCore.Commands.Abstract;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Tabs.AddAccount
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class AddAccountCommand : CommandUIBase, ICommand<AccountInput>
    {
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IUseragentManager _useragentManager;

        public AddAccountCommand(IDialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, IMediator mediator, IValidator<AccountInput> accountInputValidator, IDbContextFactory<AppDbContext> contextFactory, IUseragentManager useragentManager) : base(dialogService, waitingOverlayViewModel, mediator)
        {
            _accountInputValidator = accountInputValidator;
            _contextFactory = contextFactory;
            _useragentManager = useragentManager;
        }

        public async Task<Result> Execute(AccountInput accountInput, CancellationToken cancellationToken)
        {
            var results = await _accountInputValidator.ValidateAsync(accountInput, cancellationToken);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return Result.Ok();
            }

            await _waitingOverlayViewModel.Show("adding account");

            var dto = accountInput.ToDto();
            var success = Add(dto);
            if (success) await _mediator.Publish(new AccountUpdated(), cancellationToken);

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", success ? "Added account" : "Account is duplicated");

            return Result.Ok();
        }

        private bool Add(AccountDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var isExist = context.Accounts
                .Where(x => x.Username == dto.Username)
                .Where(x => x.Server == dto.Server)
                .Any();

            if (isExist) return false;

            var account = dto.ToEntity();
            foreach (var access in account.Accesses.Where(access => string.IsNullOrEmpty(access.Useragent)))
            {
                access.Useragent = _useragentManager.Get();
            }

            context.Add(account);
            context.SaveChanges();
            context.FillAccountSettings(new(account.Id));
            return true;
        }
    }
}