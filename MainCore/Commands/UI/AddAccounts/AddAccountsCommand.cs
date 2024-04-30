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
        private readonly UnitOfRepository _unitOfRepository;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;

        public AddAccountsCommandHandler(IDialogService dialogService, IMediator mediator, UnitOfRepository unitOfRepository, WaitingOverlayViewModel waitingOverlayViewModel)
        {
            _dialogService = dialogService;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Handle(AddAccountsCommand request, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.Show("adding accounts");

            var accounts = request.Accounts;
            _unitOfRepository.AccountRepository.Add(accounts);

            await _mediator.Publish(new AccountUpdated(), cancellationToken);

            await _waitingOverlayViewModel.Hide();

            _dialogService.ShowMessageBox("Information", "Added accounts");
        }
    }
}