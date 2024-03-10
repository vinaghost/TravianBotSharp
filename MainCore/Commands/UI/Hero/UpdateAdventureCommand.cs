using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Commands.UI.Hero
{
    public class UpdateAdventureCommand : ByAccountIdBase, IRequest
    {
        public UpdateAdventureCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class UpdateAdventureCommandHandler : IRequestHandler<UpdateAdventureCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public UpdateAdventureCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(UpdateAdventureCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            await _taskManager.AddOrUpdate<UpdateAdventureTask>(accountId);
            _dialogService.ShowMessageBox("Information", "Added update adventure task");
        }
    }
}