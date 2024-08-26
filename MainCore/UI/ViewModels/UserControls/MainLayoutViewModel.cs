using MainCore.UI.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using Serilog;
using System.Reactive.Linq;
using System.Reflection;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterAsViewModel]
    public class MainLayoutViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogService _logService;
        private readonly ITimerManager _timerManager;
        private readonly IChromeManager _chromeManager;

        private readonly AccountTabStore _accountTabStore;
        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, IMediator mediator, ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory, ILogService logService, ITimerManager timerManager, IChromeManager chromeManager, IDialogService dialogService)
        {
            _accountTabStore = accountTabStore;
            _mediator = mediator;
            _taskManager = taskManager;

            _contextFactory = contextFactory;
            _logService = logService;
            _timerManager = timerManager;
            _chromeManager = chromeManager;
            _dialogService = dialogService;

            var isEnable = this.WhenAnyValue(x => x.Accounts.IsEnable);
            AddAccount = ReactiveCommand.Create(AddAccountHandler, isEnable);
            AddAccounts = ReactiveCommand.Create(AddAccountsHandler, isEnable);
            DeleteAccount = ReactiveCommand.CreateFromTask(DeleteAccountHandler, isEnable);

            Login = ReactiveCommand.CreateFromTask(LoginHandler, isEnable);
            Logout = ReactiveCommand.CreateFromTask(LogoutTask, isEnable);
            Pause = ReactiveCommand.CreateFromTask(PauseTask, isEnable);
            Restart = ReactiveCommand.CreateFromTask(RestartTask, isEnable);

            LoadVersion = ReactiveCommand.Create(LoadVersionHandler);
            LoadAccount = ReactiveCommand.Create(LoadAccountHandler);
            GetAccount = ReactiveCommand.Create<AccountId, ListBoxItem>(GetAccountHandler);
            GetStatus = ReactiveCommand.Create<AccountId, StatusEnums>(GetStatusHandler);

            var accountObservable = this.WhenAnyValue(x => x.Accounts.SelectedItem);
            accountObservable.BindTo(selectedItemStore, vm => vm.Account);

            accountObservable.Subscribe(x =>
            {
                var tabType = AccountTabType.Normal;
                if (x is null) tabType = AccountTabType.NoAccount;
                _accountTabStore.SetTabType(tabType);
            });

            accountObservable
                .Select(x =>
                {
                    if (x is null) return AccountId.Empty;
                    return new AccountId(x.Id);
                })
                .InvokeCommand(GetStatus);

            LoadVersion
                .Do(version => Log.Information("===============> Current version: {Version} <===============", version))
                .ToProperty(this, x => x.Version, out _version);

            LoadAccount.Subscribe(accounts => Accounts.Load(accounts));

            GetStatus.Subscribe(SetPauseText);

            Observable
                .Merge(
                    Login.IsExecuting.Select(x => !x),
                    Logout.IsExecuting.Select(x => !x),
                    Pause.IsExecuting.Select(x => !x),
                    Restart.IsExecuting.Select(x => !x)
                )
                .BindTo(Accounts, x => x.IsEnable);
        }

        public async Task Load()
        {
            await LoadVersion.Execute();
            await LoadAccount.Execute();
        }

        private void AddAccountHandler()
        {
            Accounts.SelectedItem = null;
            _accountTabStore.SetTabType(AccountTabType.AddAccount);
        }

        private void AddAccountsHandler()
        {
            Accounts.SelectedItem = null;
            _accountTabStore.SetTabType(AccountTabType.AddAccounts);
        }

        private async Task DeleteAccountHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(Accounts.SelectedItemId);

            var status = _taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                _dialogService.ShowMessageBox("Warning", "Account should be offline");
                return;
            }
            var result = _dialogService.ShowConfirmBox("Information", $"Are you sure want to delete \n {Accounts.SelectedItem.Content}");
            if (!result) return;

            DeleteAccountFromDatabase(accountId);

            await _mediator.Publish(new AccountUpdated());
        }

        private async Task LoginHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(Accounts.SelectedItemId);

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

            var result = await new GetAccess().Execute(accountId, true);

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

            var chromeBrowser = _chromeManager.Get(accountId);

            result = await new OpenBrowserCommand().Execute(chromeBrowser, accountId, access, CancellationToken.None);
            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                var errors = result.Errors.Select(x => x.Message).ToList();
                logger.Error("{Errors}", string.Join(Environment.NewLine, errors));
                await _taskManager.SetStatus(accountId, StatusEnums.Offline);
                await chromeBrowser.Close();
                return;
            }
            await _mediator.Publish(new AccountInit(accountId));

            _timerManager.Start(accountId);
            await _taskManager.SetStatus(accountId, StatusEnums.Online);
        }

        private async Task LogoutTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
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

        private async Task<StatusEnums> PauseTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return StatusEnums.Offline;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
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

        private async Task RestartTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(Accounts.SelectedItemId);
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
                    await _taskManager.SetStatus(accountId, StatusEnums.Starting);
                    await _taskManager.Clear(accountId);
                    await _mediator.Publish(new AccountInit(accountId));
                    await _taskManager.SetStatus(accountId, StatusEnums.Online);
                    return;
            }
        }

        public async Task LoadStatus(AccountId accountId, StatusEnums status)
        {
            GetAccount.Execute(accountId).Subscribe(account => account.Color = status.GetColor());
            if (accountId.Value != Accounts.SelectedItemId) return;
            await GetStatus.Execute(accountId);
        }

        private StatusEnums GetStatusHandler(AccountId accountId)
        {
            if (accountId == AccountId.Empty) return StatusEnums.Starting;
            return _taskManager.GetStatus(accountId);
        }

        private List<ListBoxItem> LoadAccountHandler()
        {
            var items = GetAccountItems();
            return items;
        }

        private ListBoxItem GetAccountHandler(AccountId accountId)
        {
            var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
            return account;
        }

        private string LoadVersionHandler()
        {
            var versionAssembly = Assembly.GetExecutingAssembly().GetName().Version;
            var version = new Version(versionAssembly.Major, versionAssembly.Minor, versionAssembly.Build);
            return $"{version}";
        }

        private void SetPauseText(StatusEnums status)
        {
            switch (status)
            {
                case StatusEnums.Offline:
                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    PauseText = "[~~!~~]";
                    break;

                case StatusEnums.Online:
                    PauseText = "Pause";
                    break;

                case StatusEnums.Paused:
                    PauseText = "Resume";
                    break;

                default:
                    break;
            }
        }

        private void DeleteAccountFromDatabase(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();
        }

        private List<ListBoxItem> GetAccountItems()
        {
            using var context = _contextFactory.CreateDbContext();

            var accounts = context.Accounts
                .AsEnumerable()
                .Select(x =>
                {
                    var serverUrl = new Uri(x.Server);
                    var status = _taskManager.GetStatus(new(x.Id));
                    return new ListBoxItem()
                    {
                        Id = x.Id,
                        Color = status.GetColor(),
                        Content = $"{x.Username}{Environment.NewLine}({serverUrl.Host})"
                    };
                })
                .ToList();

            return accounts;
        }

        private readonly ObservableAsPropertyHelper<string> _version;
        public string Version => _version.Value;

        private string _pauseText = "[~~!~~]";

        public string PauseText
        {
            get => _pauseText;
            set => this.RaiseAndSetIfChanged(ref _pauseText, value);
        }

        public ReactiveCommand<Unit, Unit> AddAccount { get; }
        public ReactiveCommand<Unit, Unit> AddAccounts { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccount { get; }
        public ReactiveCommand<Unit, Unit> Login { get; }
        public ReactiveCommand<Unit, Unit> Logout { get; }
        public ReactiveCommand<Unit, StatusEnums> Pause { get; }
        public ReactiveCommand<Unit, Unit> Restart { get; }
        public ReactiveCommand<Unit, string> LoadVersion { get; }
        public ReactiveCommand<Unit, List<ListBoxItem>> LoadAccount { get; }
        public ReactiveCommand<AccountId, ListBoxItem> GetAccount { get; }
        public ReactiveCommand<AccountId, StatusEnums> GetStatus { get; }
    }
}