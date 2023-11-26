using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI
{
    public class PauseAccountCommand : ByAccountIdBase, IRequest
    {
        public PauseAccountCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class PauseAccountCommandHandler : IRequestHandler<PauseAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public PauseAccountCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(PauseAccountCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;

            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Paused)
            {
                _taskManager.SetStatus(accountId, StatusEnums.Online);
                return;
            }

            if (status == StatusEnums.Online)
            {
                await _taskManager.StopCurrentTask(accountId);
                _taskManager.SetStatus(accountId, StatusEnums.Paused);
                return;
            }

            _dialogService.ShowMessageBox("Information", $"Account is {status}");
        }
    }
}