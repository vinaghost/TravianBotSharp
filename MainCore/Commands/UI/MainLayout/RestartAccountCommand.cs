using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class RestartAccountCommand : ByListBoxItemBase, IRequest
    {
        public RestartAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class RestartAccountCommandHandler : IRequestHandler<RestartAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public RestartAccountCommandHandler(ITaskManager taskManager, IDialogService dialogService, IMediator mediator)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _mediator = mediator;
        }

        public async Task Handle(RestartAccountCommand request, CancellationToken cancellationToken)
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
                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    _dialogService.ShowMessageBox("Information", $"Account is {status}");
                    return;

                case StatusEnums.Online:
                    _dialogService.ShowMessageBox("Information", $"Account should be paused first");
                    return;

                case StatusEnums.Paused:
                    await Handle(accountId);
                    return;
            }
        }

        private async Task Handle(AccountId accountId)
        {
            await _taskManager.SetStatus(accountId, StatusEnums.Starting);

            await _taskManager.Clear(accountId);

            await _mediator.Publish(new AccountInit(accountId));

            await _taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}