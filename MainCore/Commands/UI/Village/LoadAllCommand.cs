using MainCore.Tasks;

namespace MainCore.Commands.UI.Village
{
    public class LoadAllCommand : ByAccountIdBase, IRequest
    {
        public LoadAllCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class LoadAllCommandHandler : IRequestHandler<LoadAllCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly UnitOfRepository _unitOfRepository;

        public LoadAllCommandHandler(ITaskManager taskManager, IDialogService dialogService, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(LoadAllCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villages = _unitOfRepository.VillageRepository.Get(accountId);
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
            _dialogService.ShowMessageBox("Information", $"Added update task");
        }
    }
}