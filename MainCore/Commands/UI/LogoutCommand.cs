namespace MainCore.Commands.UI.Tabs
{
    [RegisterSingleton<LogoutCommand>]
    public class LogoutCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        public LogoutCommand(IChromeManager chromeManager, IDialogService dialogService, ITaskManager taskManager)
        {
            _chromeManager = chromeManager;
            _dialogService = dialogService;
            _taskManager = taskManager;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
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

            await _taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await _taskManager.StopCurrentTask(accountId);

            var chromeBrowser = _chromeManager.Get(accountId);
            await chromeBrowser.Close();

            await _taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}