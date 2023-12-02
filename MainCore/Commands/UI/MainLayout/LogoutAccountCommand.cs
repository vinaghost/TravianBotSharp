using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class LogoutAccountCommand : ByListBoxItemBase, IRequest
    {
        public LogoutAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class LogoutAccountCommandHandler : IRequestHandler<LogoutAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly ICloseBrowserCommand _closeCommand;
        private readonly IDialogService _dialogService;

        public LogoutAccountCommandHandler(ITaskManager taskManager, ICloseBrowserCommand closeCommand, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _closeCommand = closeCommand;
            _dialogService = dialogService;
        }

        public async Task Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(accounts.SelectedItemId);
            var status = _taskManager.GetStatus(accountId);
            switch (status)
            {
                case StatusEnums.Offline:
                    _dialogService.ShowMessageBox("Warning", "Account's browser is already closed");
                    return;

                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    _dialogService.ShowMessageBox("Warning", $"TBS is {status}. Please waiting");
                    return;

                case StatusEnums.Online:
                case StatusEnums.Paused:
                    break;

                default:
                    break;
            }

            _taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await _taskManager.StopCurrentTask(accountId);

            await _closeCommand.Execute(accountId);

            _taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}