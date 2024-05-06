using FluentValidation;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Account
{
    public class AddAccountCommand : IRequest
    {
        public AddAccountCommand(AccountInput accountInput)
        {
            AccountInput = accountInput;
        }

        public AccountInput AccountInput { get; }
    }

    public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand>
    {
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly IDialogService _dialogService;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IMediator _mediator;
        private readonly IAccountRepository _accountRepository;

        public AddAccountCommandHandler(IValidator<AccountInput> accountInputValidator, IDialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, IMediator mediator, IAccountRepository accountRepository)
        {
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _mediator = mediator;
            _accountRepository = accountRepository;
        }

        public async Task Handle(AddAccountCommand request, CancellationToken cancellationToken)
        {
            var accountInput = request.AccountInput;
            var results = _accountInputValidator.Validate(accountInput);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return;
            }
            await _waitingOverlayViewModel.Show("adding account");

            var dto = accountInput.ToDto();
            var success = await Task.Run(() => _accountRepository.Add(dto));
            if (success) await _mediator.Publish(new AccountUpdated(), cancellationToken);

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", success ? "Added account" : "Account is duplicated");
        }
    }
}