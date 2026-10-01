using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MainCore.UI.ViewModels.UserControls
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Concurrency;
    using ReactiveUI.Primitives.Disposables;
    using ReactiveUI.Primitives.Extensions;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<MainLayoutViewModel>]
    public partial class MainLayoutViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly ITimerManager _timerManager;
        private readonly ILogger _logger;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly SelectedItemStore _selectedItemStore;
        private readonly IRxQueue _rxQueue;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        private readonly AccountTabStore _accountTabStore;
        private readonly IObservable<bool> _canExecute;
        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, IDialogService dialogService, ITaskManager taskManager, ILogger logger, IRxQueue rxQueue, IDbContextFactory<AppDbContext> contextFactory, ITimerManager timerManager, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _accountTabStore = accountTabStore;
            _serviceScopeFactory = serviceScopeFactory;
            _dialogService = dialogService;
            _rxQueue = rxQueue;
            _logger = logger.ForContext<MainLayoutViewModel>();

            _taskManager = taskManager;
            _timerManager = timerManager;
            _contextFactory = contextFactory;
            _selectedItemStore = selectedItemStore;

            _canExecute = this.WhenAnyValue(x => x.Accounts.IsEnable);
            _versionHelper = LoadVersionCommand
                .Do(version => _logger.Information("===============> Current version: {Version} <===============", version))
                .ToProperty(this, x => x.Version);
            Init();
        }

        private void Init()
        {
            var accountObservable = this.WhenAnyValue(x => x.Accounts.SelectedItem);
            accountObservable.BindTo(_selectedItemStore, vm => vm.Account);

            accountObservable.Subscribe(x =>
            {
                var tabType = AccountTabType.Normal;
                if (x is null) tabType = AccountTabType.NoAccount;
                _accountTabStore.SetTabType(tabType);
            });

            accountObservable
                .WhereNotNull()
                .Select(x => new AccountId(x.Id))
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .InvokeCommand(GetStatusCommand);

            LoadAccountCommand.Subscribe(Accounts.Load);
            GetStatusCommand.Subscribe(SetPauseText);

            DeleteAccountCommand.InvokeCommand(LoadAccountCommand);

            Signal
                .Merge(
                    LoginCommand.IsExecuting.Select(x => !x),
                    LogoutCommand.IsExecuting.Select(x => !x),
                    PauseCommand.IsExecuting.Select(x => !x),
                    RestartCommand.IsExecuting.Select(x => !x)
                )
                .BindTo(Accounts, x => x.IsEnable);

            _rxQueue.RegisterCommand(StatusModifiedCommand);

            _rxQueue.GetObservable<AccountsModified>()
                .Select(x => RxVoid.Default)
                .InvokeCommand(LoadAccountCommand);
        }

        public async Task Load()
        {
            await LoadVersionCommand.Execute();
            await LoadAccountCommand.Execute();
        }

        [ReactiveCommand]
        private void StatusModified(StatusModified notification)
        {
            if (Accounts.SelectedItem is null) return;

            var (accountId, status) = notification;

            var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
            if (account is null) return;
            account.Color = status.GetColor();

            RxSchedulers.MainThreadScheduler.Schedule(
                state: (this, status),
                action: static (sequencer, state) =>
                {
                    var (@this, status) = state;
                    @this.SetPauseText(status);
                    return EmptyDisposable.Instance;
                });
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        private void AddAccount()
        {
            Accounts.SelectedItem = null;
            _accountTabStore.SetTabType(AccountTabType.AddAccount);
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        private void AddAccounts()
        {
            Accounts.SelectedItem = null;
            _accountTabStore.SetTabType(AccountTabType.AddAccounts);
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute), RunInBackground = true)]
        private async Task DeleteAccount()
        {
            if (Accounts.SelectedItem is null)
            {
                await _dialogService.SendMessage("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItem.Id);
            var status = _taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                await _dialogService.SendMessage("Warning", "Account should be offline");
                return;
            }

            var result = await _dialogService.SendConfirm("Information", $"Are you sure want to delete \n {Accounts.SelectedItem.Content}");
            if (!result) return;

            using var context = _contextFactory.CreateDbContext();
            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute), RunInBackground = true)]
        private async Task Login()
        {
            if (Accounts.SelectedItem is null)
            {
                await _dialogService.SendMessage("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItem.Id);

            using var context = _contextFactory.CreateDbContext();
            var tribe = (TribeEnums)context.ByName(accountId, AccountSettingEnums.Tribe);
            if (tribe == TribeEnums.Any)
            {
                await _dialogService.SendMessage("Warning", "Choose tribe first");
                return;
            }

            if (_taskManager.GetStatus(accountId) != StatusEnums.Offline)
            {
                await _dialogService.SendMessage("Warning", "Account should be offline");
                return;
            }

            var result = await context.GetValidAccess(accountId);
            if (result.IsFailed)
            {
                await _dialogService.SendMessage("Warning", string.Join(Environment.NewLine, result.Errors.Select(x => x.Message)));
                return;
            }

            _taskManager.SetStatus(accountId, StatusEnums.Starting);

            try
            {
                using var scope = _serviceScopeFactory.CreateScope(accountId);
                var openBrowserCommand = scope.ServiceProvider.GetRequiredService<OpenBrowserCommand.Handler>();
                await openBrowserCommand.HandleAsync(new(accountId, result.Value));
            }
            catch (Exception ex)
            {
                await _dialogService.SendMessage("Error", $"Failed to open browser: {ex.Message}");
                _taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }

            _timerManager.Start(accountId);
            _taskManager.SetStatus(accountId, StatusEnums.Online);
            _rxQueue.Enqueue(new AccountInit(accountId));
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute), RunInBackground = true)]
        private async Task Logout()
        {
            if (Accounts.SelectedItem is null)
            {
                await _dialogService.SendMessage("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItem.Id);
            var status = _taskManager.GetStatus(accountId);
            switch (status)
            {
                case StatusEnums.Offline:
                    await _dialogService.SendMessage("Warning", "Account's browser is already closed");
                    return;

                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    await _dialogService.SendMessage("Warning", $"TBS is {status}. Please waiting");
                    return;

                case StatusEnums.Online:
                case StatusEnums.Paused:
                default:
                    break;
            }

            using var scope = _serviceScopeFactory.CreateScope(accountId);
            var browser = scope.ServiceProvider.GetRequiredService<IChromeBrowser>();

            _taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await _taskManager.StopCurrentTask(accountId);
            await browser.Shutdown();
            _taskManager.SetStatus(accountId, StatusEnums.Offline);
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute), RunInBackground = true)]
        private async Task Pause()
        {
            if (Accounts.SelectedItem is null)
            {
                await _dialogService.SendMessage("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItem.Id);

            var status = _taskManager.GetStatus(accountId);
            switch (status)
            {
                case StatusEnums.Paused:
                    _taskManager.SetStatus(accountId, StatusEnums.Online);
                    break;

                case StatusEnums.Online:
                    await _taskManager.StopCurrentTask(accountId);
                    break;

                case StatusEnums.Offline:
                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    await _dialogService.SendMessage("Information", $"Account is {status}");
                    break;

                default:
                    break;
            }
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute), RunInBackground = true)]
        private async Task Restart()
        {
            if (Accounts.SelectedItem is null)
            {
                await _dialogService.SendMessage("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItem.Id);
            var status = _taskManager.GetStatus(accountId);

            switch (status)
            {
                case StatusEnums.Offline:
                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    await _dialogService.SendMessage("Information", $"Account is {status}");
                    return;

                case StatusEnums.Online:
                    await _dialogService.SendMessage("Information", "Account should be paused first");
                    return;

                case StatusEnums.Paused:
                    _taskManager.SetStatus(accountId, StatusEnums.Starting);
                    await Task.Delay(300);
                    _taskManager.Clear(accountId);
                    _rxQueue.Enqueue(new AccountInit(accountId));
                    _taskManager.SetStatus(accountId, StatusEnums.Online);
                    return;
            }
        }

        [ReactiveCommand(RunInBackground = true)]
        private StatusEnums GetStatus(AccountId accountId)
        {
            if (accountId == AccountId.Empty) return StatusEnums.Starting;
            return _taskManager.GetStatus(accountId);
        }

        [ReactiveCommand(RunInBackground = true)]
        private List<ListBoxItem> LoadAccount()
        {
            using var context = _contextFactory.CreateDbContext();
            var items = context.Accounts
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
            return items;
        }

        [ReactiveCommand(RunInBackground = true)]
        private static string LoadVersion()
        {
            var versionAssembly = Assembly.GetExecutingAssembly().GetName().Version!;
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

        [ObservableAsProperty]
        private string _version = "";

        [Reactive]
        private string _pauseText = "[~~!~~]";
    }
}