using MainCore.Commands.UI.MainLayout;
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
    [RegisterAsSingleton(withoutInterface: true)]
    public class MainLayoutViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly ITaskManager _taskManager;
        private readonly IAccountRepository _accountRepository;

        private readonly AccountTabStore _accountTabStore;
        private readonly SelectedItemStore _selectedItemStore;
        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, IMediator mediator, ITaskManager taskManager, IAccountRepository accountRepository)
        {
            _accountTabStore = accountTabStore;
            _selectedItemStore = selectedItemStore;
            _mediator = mediator;
            _taskManager = taskManager;
            _accountRepository = accountRepository;

            var isEnable = this.WhenAnyValue(x => x.Accounts.IsEnable);
            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler, isEnable);
            AddAccounts = ReactiveCommand.CreateFromTask(AddAccountsHandler, isEnable);
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
            accountObservable.BindTo(_selectedItemStore, vm => vm.Account);

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
                .Do(version => Log.Information("===============> Current version: {version} <===============", version))
                .ToProperty(this, x => x.Version, out _version);

            LoadAccount.Subscribe(accounts => Accounts.Load(accounts));

            GetStatus.Subscribe(SetPauseText);
            Observable
                .Merge(new IObservable<bool>[] {
                    Login.IsExecuting.Select(x => !x),
                    Logout.IsExecuting.Select(x => !x),
                    Pause.IsExecuting.Select(x => !x),
                    Restart.IsExecuting.Select(x => !x),
                })
                .BindTo(Accounts, x => x.IsEnable);
        }

        public async Task Load()
        {
            await LoadVersion
                 .Execute();

            await LoadAccount
                .Execute();
        }

        private async Task AddAccountHandler()
        {
            await _mediator.Send(new AddAccountCommand(Accounts));
        }

        private async Task AddAccountsHandler()
        {
            await _mediator.Send(new AddAccountsCommand(Accounts));
        }

        private async Task DeleteAccountHandler()
        {
            await _mediator.Send(new DeleteAccountCommand(Accounts));
        }

        private async Task LoginHandler()
        {
            await _mediator.Send(new LoginAccountCommand(Accounts));
        }

        private async Task LogoutTask()
        {
            await _mediator.Send(new LogoutAccountCommand(Accounts));
        }

        private async Task<StatusEnums> PauseTask()
        {
            return await _mediator.Send(new PauseAccountCommand(Accounts));
        }

        private async Task RestartTask()
        {
            await _mediator.Send(new RestartAccountCommand(Accounts));
        }

        public async Task LoadStatus(AccountId accountId, StatusEnums status)
        {
            GetAccount
                .Execute(accountId)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(account => account.Color = status.GetColor());
            if (accountId.Value != Accounts.SelectedItemId) return;
            await GetStatus
                .Execute(accountId)
                .ObserveOn(RxApp.TaskpoolScheduler);
        }

        private StatusEnums GetStatusHandler(AccountId accountId)
        {
            if (accountId == AccountId.Empty) return StatusEnums.Starting;
            return _taskManager.GetStatus(accountId);
        }

        private List<ListBoxItem> LoadAccountHandler()
        {
            var items = _accountRepository.GetItems();
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