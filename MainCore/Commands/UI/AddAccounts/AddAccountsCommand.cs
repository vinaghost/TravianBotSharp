using MainCore.DTO;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.AddAccounts
{
    public class AddAccountsCommand : IRequest
    {
        public List<AccountDetailDto> Accounts { get; }

        public AddAccountsCommand(List<AccountDetailDto> accounts)
        {
            Accounts = accounts;
        }
    }

    public class AddAccountsCommandHandler : IRequestHandler<AddAccountsCommand>
    {
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IAccountRepository _accountRepository;

        public AddAccountsCommandHandler(IDialogService dialogService, IMediator mediator, WaitingOverlayViewModel waitingOverlayViewModel, IAccountRepository accountRepository)
        {
            _dialogService = dialogService;
            _mediator = mediator;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _accountRepository = accountRepository;
        }

        public async Task Handle(AddAccountsCommand request, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.Show("adding accounts");

            var accounts = request.Accounts;
            _accountRepository.Add(accounts);

            await _mediator.Publish(new AccountUpdated(), cancellationToken);

            await _waitingOverlayViewModel.Hide();

            _dialogService.ShowMessageBox("Information", "Added accounts");
        }
    }
}