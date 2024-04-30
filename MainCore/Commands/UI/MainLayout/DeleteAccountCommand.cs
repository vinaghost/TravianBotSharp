using MainCore.Common.MediatR;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.MainLayout
{
    public class DeleteAccountCommand : ByListBoxItemBase, IRequest
    {
        public DeleteAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
    {
        private readonly IMediator _mediator;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        public DeleteAccountCommandHandler(IMediator mediator, UnitOfRepository unitOfRepository, IDialogService dialogService, ITaskManager taskManager)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
            _taskManager = taskManager;
        }

        public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(accounts.SelectedItemId);

            var status = _taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                _dialogService.ShowMessageBox("Warning", "Account should be offline");
                return;
            }
            var result = _dialogService.ShowConfirmBox("Information", $"Are you sure want to delete \n {accounts.SelectedItem.Content}");
            if (!result) return;

            await Task.Run(() => _unitOfRepository.AccountRepository.Delete(accountId), cancellationToken);

            await _mediator.Publish(new AccountUpdated(), cancellationToken);
        }
    }
}