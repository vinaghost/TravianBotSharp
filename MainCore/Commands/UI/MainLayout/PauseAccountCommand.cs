using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class PauseAccountCommand : ByListBoxItemBase, IRequest<StatusEnums>
    {
        public PauseAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class PauseAccountCommandHandler : IRequestHandler<PauseAccountCommand, StatusEnums>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public PauseAccountCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task<StatusEnums> Handle(PauseAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;

            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return StatusEnums.Offline;
            }

            var accountId = new AccountId(accounts.SelectedItemId);
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Paused)
            {
                await _taskManager.SetStatus(accountId, StatusEnums.Online);
                return StatusEnums.Online;
            }

            if (status == StatusEnums.Online)
            {
                await _taskManager.StopCurrentTask(accountId);
                await _taskManager.SetStatus(accountId, StatusEnums.Paused);
                return StatusEnums.Paused;
            }

            _dialogService.ShowMessageBox("Information", $"Account is {status}");
            return status;
        }
    }
}