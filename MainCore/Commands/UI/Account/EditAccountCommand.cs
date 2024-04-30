using FluentValidation;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Account
{
    public class EditAccountCommand : IRequest
    {
        public EditAccountCommand(AccountInput accountInput)
        {
            AccountInput = accountInput;
        }

        public AccountInput AccountInput { get; }
    }

    public class EditAccountCommandHandler : IRequestHandler<EditAccountCommand>
    {
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly IDialogService _dialogService;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        public EditAccountCommandHandler(IValidator<AccountInput> accountInputValidator, IDialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, UnitOfRepository unitOfRepository, IMediator mediator)
        {
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public async Task Handle(EditAccountCommand request, CancellationToken cancellationToken)
        {
            var accountInput = request.AccountInput;
            var results = _accountInputValidator.Validate(accountInput);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return;
            }

            await _waitingOverlayViewModel.Show("editing account");

            var dto = accountInput.ToDto();
            await Task.Run(() => _unitOfRepository.AccountRepository.Update(dto), cancellationToken);
            await _mediator.Publish(new AccountUpdated(), cancellationToken);

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", "Edited account");
        }
    }
}