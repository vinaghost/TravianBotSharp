using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Commands.UI.Farming
{
    public class StartFarmListCommand : ByAccountIdBase, IRequest
    {
        public StartFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class StartFarmListCommandHandler : IRequestHandler<StartFarmListCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IUnitOfRepository _unitOfRepository;

        public StartFarmListCommandHandler(ITaskManager taskManager, IDialogService dialogService, IUnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(StartFarmListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var useStartAllButton = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, Common.Enums.AccountSettingEnums.UseStartAllButton);
            if (!useStartAllButton)
            {
                var count = _unitOfRepository.FarmRepository.CountActive(accountId);
                if (count == 0)
                {
                    _dialogService.ShowMessageBox("Information", "There is no active farm or use start all button is disable");
                    return;
                }
            }
            await _taskManager.AddOrUpdate<StartFarmListTask>(accountId);
            _dialogService.ShowMessageBox("Information", "Added start farm list task");
        }
    }
}