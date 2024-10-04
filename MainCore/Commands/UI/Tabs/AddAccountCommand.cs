using FluentValidation;
using MainCore.Commands.Abstract;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Tabs
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class AddAccountCommand : CommandUIBase, ICommand<AccountInput>, ICommand<List<AccountDetailDto>>
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

            if (IsExist(dto))
            {
                _dialogService.ShowMessageBox("Information", "Account is duplicated");
                await _waitingOverlayViewModel.Hide();
                return Result.Ok();
            }

            Add(dto);

            await _mediator.Publish(new AccountUpdated(), cancellationToken);
            _dialogService.ShowMessageBox("Information", "Added account");
            await _waitingOverlayViewModel.Hide();

            return Result.Ok();
        }

        public async Task<Result> Execute(List<AccountDetailDto> accountDetails, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.Show("adding accounts");

            var dtos = IsExist(accountDetails);

            if (dtos.Count == 0)
            {
                _dialogService.ShowMessageBox("Information", "All accounts are duplicated");
                await _waitingOverlayViewModel.Hide();
                return Result.Ok();
            }
            dtos.ForEach(Add);

            await _mediator.Publish(new AccountUpdated(), cancellationToken);
            _dialogService.ShowMessageBox("Information", $"Added {dtos.Count} accounts");
            await _waitingOverlayViewModel.Hide();
            return Result.Ok();
        }

        private bool IsExist(AccountDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            return context.Accounts
                .Where(x => x.Username == dto.Username)
                .Where(x => x.Server == dto.Server)
                .Any();
        }

        private List<AccountDto> IsExist(List<AccountDetailDto> accountDetails)
        {
            using var context = _contextFactory.CreateDbContext();

            var accounts = context.Accounts
            .ToDto()
                .ToList();

            var dtos = accountDetails
                .Select(x => x.ToDto())
                .ToList();

            return dtos
                .Where(dto => !accounts.Exists(x => x.Username == dto.Username && x.Server == dto.Server))
                .ToList();
        }

        private void Add(AccountDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var account = dto.ToEntity();
            foreach (var access in account.Accesses.Where(access => string.IsNullOrEmpty(access.Useragent)))
            {
                access.Useragent = _useragentManager.Get();
            }

            context.Add(account);
            context.SaveChanges();
            context.FillAccountSettings(new(account.Id));
        }
    }
}