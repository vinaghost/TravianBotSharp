using MainCore.Common.MediatR;
using MainCore.Tasks;

namespace MainCore.Commands.UI.Farming
{
    public class UpdateFarmListCommand : ByAccountIdBase, IRequest
    {
        public UpdateFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class UpdateFarmListCommandHandler : IRequestHandler<UpdateFarmListCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public UpdateFarmListCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(UpdateFarmListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            await _taskManager.AddOrUpdate<UpdateFarmListTask>(accountId);
            _dialogService.ShowMessageBox("Information", "Added update farm list task");
        }
    }
}