namespace MainCore.Commands.UI
{
    [RegisterSingleton<LoginCommand>]
    public class LoginCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly IMediator _mediator;
        private readonly ILogService _logService;
        private readonly ITimerManager _timerManager;

        public LoginCommand(IChromeManager chromeManager, IDialogService dialogService, ITaskManager taskManager, IMediator mediator, ILogService logService, ITimerManager timerManager)
        {
            _chromeManager = chromeManager;
            _dialogService = dialogService;
            _taskManager = taskManager;
            _mediator = mediator;
            _logService = logService;
            _timerManager = timerManager;
        }

        public async Task Execute(AccountId accountId, CancellationToken cancellationToken)
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var tribe = (TribeEnums)getSetting.ByName(accountId, AccountSettingEnums.Tribe);
            if (tribe == TribeEnums.Any)
            {
                _dialogService.ShowMessageBox("Warning", "Choose tribe first");
                return;
            }

            if (_taskManager.GetStatus(accountId) != StatusEnums.Offline)
            {
                _dialogService.ShowMessageBox("Warning", "Account's browser is already opened");
                return;
            }

            await _taskManager.SetStatus(accountId, StatusEnums.Starting);
            var logger = _logService.GetLogger(accountId);

            var getAccess = Locator.Current.GetService<GetAccess>();
            var result = await getAccess.Execute(accountId, true);

            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                var errors = result.Errors.Select(x => x.Message).ToList();
                logger.Error("{Errors}", string.Join(Environment.NewLine, errors));

                await _taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }
            var access = result.Value;

            logger.Information("Using connection {Proxy} to start chrome", access.Proxy);

            var openBrowserCommand = Locator.Current.GetService<OpenBrowserCommand>();
            result = await openBrowserCommand.Execute(accountId, access, cancellationToken);
            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                var errors = result.Errors.Select(x => x.Message).ToList();
                logger.Error("{Errors}", string.Join(Environment.NewLine, errors));
                await _taskManager.SetStatus(accountId, StatusEnums.Offline);

                var chromeBrowser = _chromeManager.Get(accountId);
                await chromeBrowser.Close();

                return;
            }
            await _mediator.Publish(new AccountInit(accountId), CancellationToken.None);

            _timerManager.Start(accountId);
            await _taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}