using MainCore.Commands.UI.MainLayoutViewModel;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterSingleton<MainLayoutViewModel>]
    public partial class MainLayoutViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;
        private readonly ITaskManager _taskManager;

        private readonly AccountTabStore _accountTabStore;
        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        private IObservable<bool> _canExecute;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, IDialogService dialogService, ITaskManager taskManager, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _accountTabStore = accountTabStore;
            _dialogService = dialogService;
            _serviceScopeFactory = serviceScopeFactory;

            taskManager.StatusUpdated += LoadStatus;
            _taskManager = taskManager;

            _canExecute = this.WhenAnyValue(x => x.Accounts.IsEnable);

            var accountObservable = this.WhenAnyValue(x => x.Accounts.SelectedItem);
            accountObservable.BindTo(selectedItemStore, vm => vm.Account);

            accountObservable.Subscribe(x =>
            {
                var tabType = AccountTabType.Normal;
                if (x is null) tabType = AccountTabType.NoAccount;
                _accountTabStore.SetTabType(tabType);
            });

            accountObservable
                .WhereNotNull()
                .Select(x => new AccountId(x.Id))
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(GetStatusCommand);

            _versionHelper = LoadVersionCommand
                .Do(version => Log.Information("===============> Current version: {Version} <===============", version))
                .ToProperty(this, x => x.Version);

            LoadAccountCommand.Subscribe(Accounts.Load);

            GetStatusCommand.Subscribe(SetPauseText);

            Observable
                .Merge(
                    LoginCommand.IsExecuting.Select(x => !x),
                    LogoutCommand.IsExecuting.Select(x => !x),
                    PauseCommand.IsExecuting.Select(x => !x),
                    RestartCommand.IsExecuting.Select(x => !x)
                )
                .BindTo(Accounts, x => x.IsEnable);
        }

        public async Task Load()
        {
            await LoadVersionCommand.Execute();
            await LoadAccountCommand.Execute();
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

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        private async Task DeleteAccount()
        {
            if (!Accounts.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "No account selected"));
                return;
            }
            if (Accounts.SelectedItem is null) return;

            var accountId = new AccountId(Accounts.SelectedItemId);
            using var scope = _serviceScopeFactory.CreateScope(accountId);

            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            var status = taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Account should be offline"));
                return;
            }

            var result = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Information", $"Are you sure want to delete \n {Accounts.SelectedItem.Content}"));
            if (!result) return;

            var deleteCommand = scope.ServiceProvider.GetRequiredService<DeleteCommand.Handler>();
            await deleteCommand.HandleAsync(new(accountId));
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        private async Task Login()
        {
            if (!Accounts.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "No account selected"));
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            using var scope = _serviceScopeFactory.CreateScope(accountId);

            var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();
            var tribe = (TribeEnums)settingService.ByName(accountId, AccountSettingEnums.Tribe);
            if (tribe == TribeEnums.Any)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Choose tribe first"));
                return;
            }

            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            if (taskManager.GetStatus(accountId) != StatusEnums.Offline)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Account should be offline"));
                return;
            }

            var getAccessQuery = scope.ServiceProvider.GetRequiredService<GetValidAccessQuery.Handler>();
            var result = await getAccessQuery.HandleAsync(new(accountId));
            if (result.IsFailed)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", result.ToString()));
                return;
            }

            var loginCommand = scope.ServiceProvider.GetRequiredService<LoginCommand.Handler>();
            await loginCommand.HandleAsync(new(accountId, result.Value));
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        private async Task Logout()
        {
            if (!Accounts.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "No account selected"));
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            var status = _taskManager.GetStatus(accountId);
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
                default:
                    break;
            }

            using var scope = _serviceScopeFactory.CreateScope(accountId);
            var logoutCommand = scope.ServiceProvider.GetRequiredService<LogoutCommand.Handler>();
            await logoutCommand.HandleAsync(new(accountId));
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        private async Task Pause()
        {
            if (!Accounts.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "No account selected"));
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);

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
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Account is {status}"));
                    break;

                default:
                    break;
            }
        }

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        private async Task Restart()
        {
            if (!Accounts.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "No account selected"));
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
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Account is {status}"));
                    return;

                case StatusEnums.Online:
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Account should be paused first"));
                    return;

                case StatusEnums.Paused:
                    _taskManager.SetStatus(accountId, StatusEnums.Starting);
                    _taskManager.Clear(accountId);
                    using (var scope = _serviceScopeFactory.CreateScope(accountId))
                    {
                        await scope.ServiceProvider.GetRequiredService<AccountInit.Handler>().HandleAsync(new(accountId));
                    }
                    _taskManager.SetStatus(accountId, StatusEnums.Online);
                    return;
            }
        }

        public void LoadStatus(AccountId accountId)
        {
            var status = GetStatus(accountId);
            GetAccountCommand.Execute(accountId).WhereNotNull().Subscribe(account => account.Color = status.GetColor());
            if (accountId.Value != Accounts.SelectedItemId) return;
            GetStatusCommand.Execute(accountId).Subscribe();
        }

        [ReactiveCommand]
        private StatusEnums GetStatus(AccountId accountId)
        {
            if (accountId == AccountId.Empty) return StatusEnums.Starting;
            return _taskManager.GetStatus(accountId);
        }

        [ReactiveCommand]
        private async Task<List<ListBoxItem>> LoadAccount()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var getAccountItemsQuery = scope.ServiceProvider.GetRequiredService<GetAccountItemsQuery.Handler>();
            var items = await getAccountItemsQuery.HandleAsync(new());
            return items;
        }

        [ReactiveCommand]
        private ListBoxItem? GetAccount(AccountId accountId)
        {
            var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
            return account;
        }

        [ReactiveCommand]
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