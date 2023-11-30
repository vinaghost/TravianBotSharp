using FluentValidation;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.AddAccount
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
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        public AddAccountCommandHandler(IValidator<AccountInput> accountInputValidator, IDialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, IUnitOfRepository unitOfRepository, IMediator mediator)
        {
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
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
            var success = await Task.Run(() => _unitOfRepository.AccountRepository.Add(dto));
            if (success) await _mediator.Publish(new AccountUpdated(), cancellationToken);

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", success ? "Added account" : "Account is duplicated");
        }
    }
}