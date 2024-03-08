using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Commands.UI.Hero
{
    public class UpdateInventoryCommand : ByAccountIdBase, IRequest
    {
        public UpdateInventoryCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public UpdateInventoryCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            await _taskManager.AddOrUpdate<UpdateInventoryTask>(accountId);
            _dialogService.ShowMessageBox("Information", "Added update inventory task");
        }
    }
}