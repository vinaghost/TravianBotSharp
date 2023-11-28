using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Commands.UI.Farming
{
    public class StopFarmListCommand : ByAccountIdBase, IRequest
    {
        public StopFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class StopFarmListCommandHandler : IRequestHandler<StopFarmListCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public StopFarmListCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(StopFarmListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;

            var task = _taskManager.Get<StartFarmListTask>(accountId);

            if (task is not null) await _taskManager.Remove(accountId, task);

            _dialogService.ShowMessageBox("Information", "Removed start farm list task");
        }
    }
}