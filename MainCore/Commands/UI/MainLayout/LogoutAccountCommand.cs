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

            _taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await _taskManager.StopCurrentTask(accountId);

            await _closeCommand.Execute(accountId);

            _taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}