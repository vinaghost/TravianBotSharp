using MainCore.Common.MediatR;
using MainCore.Entities;
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

        public StartFarmListCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(StartFarmListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            await _taskManager.AddOrUpdate<StartFarmListTask>(accountId);
            _dialogService.ShowMessageBox("Information", "Added start farm list task");
        }
    }
}