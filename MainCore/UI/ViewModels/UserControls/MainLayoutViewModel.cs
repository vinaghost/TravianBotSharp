using MainCore.Commands.UI;
using MainCore.Common;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Enums;
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

            AddAccountCommand = ReactiveCommand.Create(AddAccountCommandHandler);
            AddAccountsCommand = ReactiveCommand.Create(AddAccountsCommandHandler);

            DeleteAccountCommand = ReactiveCommand.CreateFromTask(DeleteAccountCommandHandler);
            LoginCommand = ReactiveCommand.CreateFromTask(LoginCommandHandler);
            LogoutCommand = ReactiveCommand.CreateFromTask(LogoutTask);
            PauseCommand = ReactiveCommand.CreateFromTask(PauseTask);
            RestartCommand = ReactiveCommand.CreateFromTask(RestartTask);

            var accountObservable = this.WhenAnyValue(x => x.Accounts.SelectedItem);
            accountObservable.BindTo(_selectedItemStore, vm => vm.Account);

            accountObservable.Subscribe(x =>
            {
                var tabType = AccountTabType.Normal;
                if (x is null) tabType = AccountTabType.NoAccount;
                _accountTabStore.SetTabType(tabType);
            });
        }

        public async Task Load()
        {
            await LoadAccountList();
            await LoadVersion();
        }

        private void AddAccountCommandHandler()
        {
            Accounts.SelectedItem = null;
            AccountTabStore.SetTabType(AccountTabType.AddAccount);
        }

        private void AddAccountsCommandHandler()
        {
            Accounts.SelectedItem = null;
            AccountTabStore.SetTabType(AccountTabType.AddAccounts);
        }

        private async Task DeleteAccountCommandHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var result = _dialogService.ShowConfirmBox("Information", $"Are you sure want to delete \n {Accounts.SelectedItem.Content}");
            if (!result) return;
            var accountId = new AccountId(Accounts.SelectedItemId);
            await Task.Run(() => _unitOfRepository.AccountRepository.Delete(accountId));
            await _mediator.Publish(new AccountUpdated());
        }

        private async Task LoginCommandHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            await _mediator.Send(new LoginAccountByIdCommand(new AccountId(Accounts.SelectedItemId)));
        }

        private async Task LogoutTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            await _mediator.Send(new LogoutAccountByIdCommand(new AccountId(Accounts.SelectedItemId)));
        }

        private async Task PauseTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            await _mediator.Send(new PauseAccountByIdCommand(new AccountId(Accounts.SelectedItemId)));
        }

        private async Task RestartTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            await _mediator.Send(new RestartAccountByIdCommand(new AccountId(Accounts.SelectedItemId)));
        }

        public async Task LoadStatus(AccountId accountId, StatusEnums status)
        {
            var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
            await Observable.Start(() =>
            {
                account.Color = status.GetColor();
            }, RxApp.MainThreadScheduler);
        }

        public async Task LoadAccountList()
        {
            var items = await Task.Run(() => _unitOfRepository.AccountRepository.GetItems());

            await Observable.Start(() =>
            {
                Accounts.Load(items);
            }, RxApp.MainThreadScheduler);
        }

        public async Task LoadVersion()
        {
            await Task.CompletedTask;
            var versionAssembly = Assembly.GetExecutingAssembly().GetName().Version;
            var version = new Version(versionAssembly.Major, versionAssembly.Minor, versionAssembly.Build);
            Version = $"{version} - {Constants.Server}";
        }

        private string _version;

        public string Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }

        public ReactiveCommand<Unit, Unit> AddAccountCommand { get; }
        public ReactiveCommand<Unit, Unit> AddAccountsCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccountCommand { get; }
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        public ReactiveCommand<Unit, Unit> RestartCommand { get; }
    }
}