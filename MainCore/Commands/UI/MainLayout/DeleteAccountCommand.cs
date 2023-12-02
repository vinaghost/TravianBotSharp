using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

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
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;

        public DeleteAccountCommandHandler(IMediator mediator, IUnitOfRepository unitOfRepository, IDialogService dialogService)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
        }

        public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var result = _dialogService.ShowConfirmBox("Information", $"Are you sure want to delete \n {accounts.SelectedItem.Content}");
            if (!result) return;

            var accountId = new AccountId(accounts.SelectedItemId);

            _unitOfRepository.AccountRepository.Delete(accountId);
            await _mediator.Publish(new AccountUpdated(), cancellationToken);
        }
    }
}