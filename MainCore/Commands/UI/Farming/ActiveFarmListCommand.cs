using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Output;
using MediatR;

namespace MainCore.Commands.UI.Farming
{
    public class ActiveFarmListCommand : ByAccountIdBase, IRequest
    {
        public ListBoxItem Item { get; }

        public ActiveFarmListCommand(AccountId accountId, ListBoxItem item) : base(accountId)
        {
            Item = item;
        }
    }

    public class ActiveFarmListCommandHandler : IRequestHandler<ActiveFarmListCommand>
    {
        private readonly IMediator _mediator;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;

        public ActiveFarmListCommandHandler(IMediator mediator, UnitOfRepository unitOfRepository, IDialogService dialogService)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
        }

        public async Task Handle(ActiveFarmListCommand request, CancellationToken cancellationToken)
        {
            var selectedFarmList = request.Item;
            if (selectedFarmList is null)
            {
                _dialogService.ShowMessageBox("Warning", "No farm list selected");
                return;
            }

            var accountId = request.AccountId;
            var farmId = new FarmId(selectedFarmList.Id);

            _unitOfRepository.FarmRepository.ChangeActive(farmId);
            await _mediator.Publish(new FarmListUpdated(accountId), cancellationToken);
        }
    }
}