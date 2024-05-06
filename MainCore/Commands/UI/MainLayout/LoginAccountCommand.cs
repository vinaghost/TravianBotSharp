using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.MainLayout
{
    public class LoginAccountCommand : ByListBoxItemBase, IRequest
    {
        public LoginAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class LoginAccountCommandHandler : IRequestHandler<LoginAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly ITimerManager _timerManager;
        private readonly IDialogService _dialogService;
        private readonly IChromeManager _chromeManager;

        private readonly GetAccess _getAccessCommand;
        private readonly OpenBrowserCommand _openBrowserCommand;

        private readonly ILogService _logService;
        private readonly IMediator _mediator;



        public LoginAccountCommandHandler(ITaskManager taskManager, ITimerManager timerManager, IDialogService dialogService, GetAccess getAccessCommand, OpenBrowserCommand openBrowserCommand, ILogService logService, IMediator mediator, IChromeManager chromeManager)
        {
            _taskManager = taskManager;
            _timerManager = timerManager;
            _dialogService = dialogService;
            _getAccessCommand = getAccessCommand;
            _openBrowserCommand = openBrowserCommand;
            _logService = logService;
            _mediator = mediator;

            _chromeManager = chromeManager;
        }

        public async Task Handle(LoginAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(accounts.SelectedItemId);

            var tribe = (TribeEnums)new GetSetting().ByName(accountId, AccountSettingEnums.Tribe);
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

            var result = await _getAccessCommand.Execute(accountId, true);

            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                var errors = result.Errors.Select(x => x.Message).ToList();
                logger.Error("{errors}", string.Join(Environment.NewLine, errors));

                await _taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }
            var access = result.Value;
            logger.Information("Using connection {proxy} to start chrome", access.Proxy);

            var chromeBrowser = _chromeManager.Get(accountId);

            result = await new OpenBrowserCommand().Execute(chromeBrowser, accountId, access, cancellationToken);
            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                var errors = result.Errors.Select(x => x.Message).ToList();
                logger.Error("{errors}", string.Join(Environment.NewLine, errors));
                await _taskManager.SetStatus(accountId, StatusEnums.Offline);
                await Task.Run(chromeBrowser.Close, CancellationToken.None);
                return;
            }
            await _mediator.Publish(new AccountInit(accountId), cancellationToken);

            _timerManager.Start(accountId);
            await _taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}