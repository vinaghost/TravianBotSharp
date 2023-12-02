using MainCore.Commands.UI.MainLayout;
using MainCore.Common;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reflection;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class MainLayoutViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        private readonly AccountTabStore _accountTabStore;
        private readonly SelectedItemStore _selectedItemStore;
        private readonly IDialogService _dialogService;

        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, IMediator mediator, IDialogService dialogService, IUnitOfRepository unitOfRepository)
        {
            _accountTabStore = accountTabStore;
            _selectedItemStore = selectedItemStore;
            _dialogService = dialogService;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);
            AddAccounts = ReactiveCommand.CreateFromTask(AddAccountsHandler);

            DeleteAccount = ReactiveCommand.CreateFromTask(DeleteAccountHandler);
            Login = ReactiveCommand.CreateFromTask(LoginHandler);
            Logout = ReactiveCommand.CreateFromTask(LogoutTask);
            Pause = ReactiveCommand.CreateFromTask(PauseTask);
            Restart = ReactiveCommand.CreateFromTask(RestartTask);

            LoadVersion = ReactiveCommand.Create(LoadVersionHandler);
            LoadAccount = ReactiveCommand.Create(LoadAccountHandler);
            GetAccount = ReactiveCommand.Create<AccountId, ListBoxItem>(GetAccountHandler);

            var accountObservable = this.WhenAnyValue(x => x.Accounts.SelectedItem);
            accountObservable.BindTo(_selectedItemStore, vm => vm.Account);

            accountObservable.Subscribe(x =>
            {
                var tabType = AccountTabType.Normal;
                if (x is null) tabType = AccountTabType.NoAccount;
                _accountTabStore.SetTabType(tabType);
            });
            LoadVersion.Subscribe(version => Version = $"{version} - {Constants.Server}");

            LoadAccount.Subscribe(accounts => Accounts.Load(accounts));
        }

        public async Task Load()
        {
            await Task.WhenAll(new Task[] {
                Task.Run(() => LoadVersion.Execute().Subscribe()),
                Task.Run(() => LoadAccount.Execute().Subscribe())
            });
        }

        private async Task AddAccountHandler()
        {
            Accounts.SelectedItem = null;
            await _mediator.Send(new AddAccountCommand());
        }

        private async Task AddAccountsHandler()
        {
            Accounts.SelectedItem = null;
            await _mediator.Send(new AddAccountsCommand());
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
            await _mediator.Send(new DeleteAccountCommand(accountId));
        }

        private async Task LoginHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            var result = await _mediator.Send(new LoginAccountCommand(accountId));

            if (result.IsFailed) _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
        }

        private async Task LogoutTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            await _mediator.Send(new LogoutAccountCommand(accountId));
        }

        private async Task PauseTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(Accounts.SelectedItemId);

            await _mediator.Send(new PauseAccountCommand(accountId));
        }

        private async Task RestartTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(Accounts.SelectedItemId);

            await _mediator.Send(new RestartAccountCommand(accountId));
        }

        public async Task LoadStatus(AccountId accountId, StatusEnums status)
        {
            await Task.Run(() => GetAccount.Execute(accountId).Subscribe(account => account.Color = status.GetColor()));
        }

        public List<ListBoxItem> LoadAccountHandler()
        {
            var items = _unitOfRepository.AccountRepository.GetItems();
            return items;
        }

        public ListBoxItem GetAccountHandler(AccountId accountId)
        {
            var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
            return account;
        }

        private Version LoadVersionHandler()
        {
            var versionAssembly = Assembly.GetExecutingAssembly().GetName().Version;
            var version = new Version(versionAssembly.Major, versionAssembly.Minor, versionAssembly.Build);

            return version;
        }

        private string _version;

        public string Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }

        public ReactiveCommand<Unit, Unit> AddAccount { get; }
        public ReactiveCommand<Unit, Unit> AddAccounts { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccount { get; }
        public ReactiveCommand<Unit, Unit> Login { get; }
        public ReactiveCommand<Unit, Unit> Logout { get; }
        public ReactiveCommand<Unit, Unit> Pause { get; }
        public ReactiveCommand<Unit, Unit> Restart { get; }
        public ReactiveCommand<Unit, Version> LoadVersion { get; }
        public ReactiveCommand<Unit, List<ListBoxItem>> LoadAccount { get; }
        public ReactiveCommand<AccountId, ListBoxItem> GetAccount { get; }
    }
}