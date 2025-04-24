using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<LogoutCommand>]
    public class LogoutCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IDialogService _dialogService;
        private readonly ITimerManager _timerManager;
        private readonly StopCurrentTask.Handler _stopCurrentTask;

        public LogoutCommand(IChromeManager chromeManager, IDialogService dialogService, ITimerManager timerManager, StopCurrentTask.Handler stopCurrentTask)
        {
            _chromeManager = chromeManager;
            _dialogService = dialogService;
            _timerManager = timerManager;
            _stopCurrentTask = stopCurrentTask;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var status = _timerManager.GetStatus(accountId);
            switch (status)
            {
                case StatusEnums.Offline:
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Account's browser is already closed"));
                    return;

                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", $"TBS is {status}. Please waiting"));
                    return;

                case StatusEnums.Online:
                case StatusEnums.Paused:
                    break;

                default:
                    break;
            }

            await _timerManager.SetStatus(accountId, StatusEnums.Stopping);
            await _stopCurrentTask.HandleAsync(new(accountId), cancellationToken);

            var chromeBrowser = _chromeManager.Get(accountId);
            await chromeBrowser.Close();

            await _timerManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}