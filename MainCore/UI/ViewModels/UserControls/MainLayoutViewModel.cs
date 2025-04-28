using MainCore.Commands.UI.MainLayoutViewModel;
using MainCore.UI.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;
using System.Reflection;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterSingleton<MainLayoutViewModel>]
    public partial class MainLayoutViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;

        private readonly AccountTabStore _accountTabStore;
        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        private IObservable<bool> _canExecute;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, IDialogService dialogService)
        {
            _accountTabStore = accountTabStore;
            _dialogService = dialogService;

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

            var taskManager = Locator.Current.GetService<ITaskManager>();
            var accountId = new AccountId(Accounts.SelectedItemId);
            var status = taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Account should be offline"));
                return;
            }

            var result = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Information", $"Are you sure want to delete \n {Accounts.SelectedItem.Content}"));
            if (!result) return;

            var deleteCommand = Locator.Current.GetService<DeleteCommand.Handler>();
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

            var getSetting = Locator.Current.GetService<IGetSetting>();
            var tribe = (TribeEnums)getSetting.ByName(accountId, AccountSettingEnums.Tribe);
            if (tribe == TribeEnums.Any)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Choose tribe first"));
                return;
            }

            var taskManager = Locator.Current.GetService<ITaskManager>();
            if (taskManager.GetStatus(accountId) != StatusEnums.Offline)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Account should be offline"));
                return;
            }

            var getAccessQuery = Locator.Current.GetService<GetAccessQuery.Handler>();
            var result = await getAccessQuery.HandleAsync(new(accountId));
            if (result.IsFailed)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", result.Errors.First().Message));
                return;
            }

            var loginCommand = Locator.Current.GetService<LoginCommand.Handler>();
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

            var taskManager = Locator.Current.GetService<ITaskManager>();
            var status = taskManager.GetStatus(accountId);
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

            var logoutCommand = Locator.Current.GetService<LogoutCommand.Handler>();
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

            var taskManager = Locator.Current.GetService<ITaskManager>();
            var status = taskManager.GetStatus(accountId);
            switch (status)
            {
                case StatusEnums.Paused:
                    await taskManager.SetStatus(accountId, StatusEnums.Online);
                    break;

                case StatusEnums.Online:
                    await taskManager.StopCurrentTast(accountId);
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
            var taskManager = Locator.Current.GetService<ITaskManager>();
            var status = taskManager.GetStatus(accountId);

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
                    await taskManager.SetStatus(accountId, StatusEnums.Starting);
                    await taskManager.Clear(accountId);
                    await Locator.Current.GetService<AccountInit.Handler>().HandleAsync(new(accountId));
                    await taskManager.SetStatus(accountId, StatusEnums.Online);
                    return;
            }
        }

        public async Task LoadStatus(AccountId accountId)
        {
            var status = GetStatus(accountId);
            GetAccountCommand.Execute(accountId).Subscribe(account => account.Color = status.GetColor());
            if (accountId.Value != Accounts.SelectedItemId) return;
            await GetStatusCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private StatusEnums GetStatus(AccountId accountId)
        {
            if (accountId == AccountId.Empty) return StatusEnums.Starting;
            var taskManager = Locator.Current.GetService<ITaskManager>();
            return taskManager.GetStatus(accountId);
        }

        [ReactiveCommand]
        private static async Task<List<ListBoxItem>> LoadAccount()
        {
            var getAccountItemsQuery = Locator.Current.GetService<GetAccountItemsQuery.Handler>();
            var items = await getAccountItemsQuery.HandleAsync(new());
            return items;
        }

        [ReactiveCommand]
        private ListBoxItem GetAccount(AccountId accountId)
        {
            var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
            return account;
        }

        [ReactiveCommand]
        private static string LoadVersion()
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

        [ObservableAsProperty]
        private string _version;

        [Reactive]
        private string _pauseText = "[~~!~~]";
    }
}