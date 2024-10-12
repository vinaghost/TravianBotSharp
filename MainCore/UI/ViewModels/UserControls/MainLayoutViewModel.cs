using MainCore.Commands.UI;
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
    [RegisterSingleton<MainLayoutViewModel>]
    public class MainLayoutViewModel : ViewModelBase
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        private readonly AccountTabStore _accountTabStore;
        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, ITaskManager taskManager, IDialogService dialogService)
        {
            _accountTabStore = accountTabStore;
            _taskManager = taskManager;
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
                .WhereNotNull()
                .Select(x => new AccountId(x.Id))
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(GetStatus);

            LoadVersion
                .Do(version => Log.Information("===============> Current version: {Version} <===============", version))
                .ToProperty(this, x => x.Version, out _version);

            LoadAccount.Subscribe(Accounts.Load);

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

            var result = _dialogService.ShowConfirmBox("Information", $"Are you sure want to delete \n {Accounts.SelectedItem.Content}");
            if (!result) return;

            var accountId = new AccountId(Accounts.SelectedItemId);
            var deleteAccountCommand = Locator.Current.GetService<DeleteAccountCommand>();
            await deleteAccountCommand.Execute(accountId, CancellationToken.None);
        }

        private async Task LoginHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            var loginCommand = Locator.Current.GetService<LoginCommand>();
            await loginCommand.Execute(accountId, CancellationToken.None);
        }

        private async Task LogoutTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);

            var logoutCommand = Locator.Current.GetService<LogoutCommand>();
            await logoutCommand.Execute(accountId, CancellationToken.None);
        }

        private async Task PauseTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            var pauseCommand = Locator.Current.GetService<PauseCommand>();
            await pauseCommand.Execute(accountId, CancellationToken.None);
        }

        private async Task RestartTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            var restartCommand = Locator.Current.GetService<RestartCommand>();
            await restartCommand.Execute(accountId, CancellationToken.None);
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

        private static List<ListBoxItem> LoadAccountHandler()
        {
            var getAccount = Locator.Current.GetService<GetAccount>();
            var items = getAccount.Items();
            return items;
        }

        private ListBoxItem GetAccountHandler(AccountId accountId)
        {
            var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
            return account;
        }

        private static string LoadVersionHandler()
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
        public ReactiveCommand<Unit, Unit> Pause { get; }
        public ReactiveCommand<Unit, Unit> Restart { get; }
        public ReactiveCommand<Unit, string> LoadVersion { get; }
        public ReactiveCommand<Unit, List<ListBoxItem>> LoadAccount { get; }
        public ReactiveCommand<AccountId, ListBoxItem> GetAccount { get; }
        public ReactiveCommand<AccountId, StatusEnums> GetStatus { get; }
    }
}