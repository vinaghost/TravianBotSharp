using FluentValidation;
using MainCore.Commands.Abstract;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Tabs
{
    public class UpdateAccountCommand : CommandUIBase, ICommand<AccountInput>
    {
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IUseragentManager _useragentManager;

        public UpdateAccountCommand(IDialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, IMediator mediator, IValidator<AccountInput> accountInputValidator, IDbContextFactory<AppDbContext> contextFactory, IUseragentManager useragentManager) : base(dialogService, waitingOverlayViewModel, mediator)
        {
            _accountInputValidator = accountInputValidator;
            _contextFactory = contextFactory;
            _useragentManager = useragentManager;
        }

        public async Task<Result> Execute(AccountInput accountInput, CancellationToken cancellationToken)
        {
            var results = await _accountInputValidator.ValidateAsync(accountInput);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return Result.Ok();
            }

            await _waitingOverlayViewModel.Show("editing account");

            var dto = accountInput.ToDto();
            Update(dto);
            await _mediator.Publish(new AccountUpdated());
            await _waitingOverlayViewModel.Hide();

            _dialogService.ShowMessageBox("Information", "Edited account");
            return Result.Ok();
        }

        private void Update(AccountDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var account = dto.ToEntity();
            foreach (var access in account.Accesses.Where(access => string.IsNullOrWhiteSpace(access.Useragent)))
            {
                access.Useragent = _useragentManager.Get();
            }

            // Remove accesses not present in the DTO
            var existingAccessIds = dto.Accesses.Select(a => a.Id.Value).ToList();
            context.Accesses
                .Where(a => a.AccountId == account.Id && !existingAccessIds.Contains(a.Id))
                .ExecuteDelete();

            context.Update(account);
            context.SaveChanges();
        }
    }
}