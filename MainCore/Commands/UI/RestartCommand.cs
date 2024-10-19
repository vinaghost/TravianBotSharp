using MainCore.UI.Models.Output;
using System.Reactive.Linq;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<RestartCommand>]
    public class RestartCommand
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public RestartCommand(ITaskManager taskManager, IDialogService dialogService, IMediator mediator)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _mediator = mediator;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var status = _taskManager.GetStatus(accountId);

            switch (status)
            {
                case StatusEnums.Offline:
                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Account is {status}"));
                    return;

                case StatusEnums.Online:
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Account should be paused first"));
                    return;

                case StatusEnums.Paused:
                    await _taskManager.SetStatus(accountId, StatusEnums.Starting);
                    await _taskManager.Clear(accountId);
                    await _mediator.Publish(new AccountInit(accountId), cancellationToken);
                    await _taskManager.SetStatus(accountId, StatusEnums.Online);
                    return;
            }
        }
    }
}