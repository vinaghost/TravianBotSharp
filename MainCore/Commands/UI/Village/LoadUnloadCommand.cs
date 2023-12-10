using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Commands.UI.Village
{
    public class LoadUnloadCommand : ByAccountIdBase, IRequest
    {
        public LoadUnloadCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class LoadUnloadCommandHandler : IRequestHandler<LoadUnloadCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly UnitOfRepository _unitOfRepository;

        public LoadUnloadCommandHandler(ITaskManager taskManager, IDialogService dialogService, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(LoadUnloadCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villages = _unitOfRepository.VillageRepository.GetMissingBuildingVillages(accountId);
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
            _dialogService.ShowMessageBox("Information", $"Added update task");
        }
    }
}