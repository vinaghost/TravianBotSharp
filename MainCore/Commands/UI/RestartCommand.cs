using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<RestartCommand>]
    public class RestartCommand
    {
        private readonly ITaskManager _taskManager;
        private readonly ITimerManager _timerManager;
        private readonly IDialogService _dialogService;
        private readonly AccountInit.Handler _accountInit;

        public RestartCommand(ITaskManager taskManager, IDialogService dialogService, AccountInit.Handler accountInit, ITimerManager timerManager)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _accountInit = accountInit;
            _timerManager = timerManager;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var status = _timerManager.GetStatus(accountId);

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
                    await _timerManager.SetStatus(accountId, StatusEnums.Starting);
                    await _taskManager.Clear(accountId);
                    await _accountInit.HandleAsync(new(accountId), cancellationToken);
                    await _timerManager.SetStatus(accountId, StatusEnums.Online);
                    return;
            }
        }
    }
}