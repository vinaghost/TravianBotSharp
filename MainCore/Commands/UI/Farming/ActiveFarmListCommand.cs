using MainCore.UI.Models.Output;

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
        private readonly IDialogService _dialogService;
        private readonly IFarmRepository _farmRepository;

        public ActiveFarmListCommandHandler(IMediator mediator, IDialogService dialogService, IFarmRepository farmRepository)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _farmRepository = farmRepository;
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

            _farmRepository.ChangeActive(farmId);
            await _mediator.Publish(new FarmListUpdated(accountId), cancellationToken);
        }
    }
}