using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI
{
    public class PauseAccountByIdCommand : ByAccountIdBase, IRequest
    {
        public PauseAccountByIdCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class PauseAccountByIdCommandHandler : IRequestHandler<PauseAccountByIdCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public PauseAccountByIdCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(PauseAccountByIdCommand request, CancellationToken cancellationToken)
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