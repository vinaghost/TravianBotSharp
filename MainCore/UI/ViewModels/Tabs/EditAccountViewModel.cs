using FluentValidation;

using MainCore.UI.Models.Input;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<EditAccountViewModel>]
    public partial class EditAccountViewModel : AccountTabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly RxQueue _rxQueue;

        public EditAccountViewModel(IValidator<AccessInput> accessInputValidator, DialogService dialogService, IValidator<AccountInput> accountInputValidator, WaitingOverlayViewModel waitingOverlayViewModel, IDbContextFactory<AppDbContext> contextFactory, RxQueue rxQueue, SelectedItemStore selectedItemStore) : base(dialogService, selectedItemStore)
        {
            _accessInputValidator = accessInputValidator;
            _accountInputValidator = accountInputValidator;
            _waitingOverlayViewModel = waitingOverlayViewModel;

            _contextFactory = contextFactory;
            _rxQueue = rxQueue;

            Init();
        }

        public void Init()
        {
            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            LoadAccountCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(SetAccount);
            DeleteAccessCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(_ => SelectedAccess = null);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadAccountCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task AddAccess()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            if (string.IsNullOrEmpty(AccountInput.Username))
            {
                AccountInput.Username = AccessInput.Username;
            }

            AccountInput.Accesses.Add(AccessInput.Clone());
        }

        [ReactiveCommand]
        private async Task EditAccess()
        {
            if (SelectedAccess is null)
            {
                await _dialogService.SendMessage("Error", "No access selected");
                return;
            }

            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            AccessInput.CopyTo(SelectedAccess);
        }

        [ReactiveCommand]
        private async Task DeleteAccess()
        {
            if (SelectedAccess is null)
            {
                await _dialogService.SendMessage("Error", "No access selected");
                return;
            }

            var confirm = await _dialogService.SendConfirm("Delete access", $"Are you sure you want to delete access {AccessInput.Username} ?");
            if (!confirm) return;

            AccountInput.Accesses.Remove(SelectedAccess);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task EditAccount()
        {
            var results = await _accountInputValidator.ValidateAsync(AccountInput);

            if (!results.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, results.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            _waitingOverlayViewModel.Show("adding account");

            try
            {
                await UpdateDatabaseCommand.Execute(AccountInput);
                _rxQueue.Enqueue(new AccountsModified());
            }
            finally
            {
                _waitingOverlayViewModel.Hide();
            }

            await _dialogService.SendMessage("Information", "Account updated");
        }

        [ReactiveCommand(RunInBackground = true)]
        private (AccountId Id, string Username, string Server, IList<AccessInput> Accesses) LoadAccount(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var account = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Select(e => new
                {
                    e.Username,
                    e.Server,
                })
                .First();

            var accesses = context.Accesses
                .Where(x => x.AccountId == accountId.Value)
                .AsEnumerable()
                .Select(x => new AccessInput()
                {
                    Id = new(x.Id),
                    Username = x.Username,
                    Password = x.Password,
                    ProxyHost = x.ProxyHost,
                    ProxyPort = x.ProxyPort,
                    ProxyUsername = x.ProxyUsername,
                    ProxyPassword = x.ProxyPassword,
                })
                .ToList();

            return (accountId, account.Username, account.Server, accesses);
        }

        private void SetAccount((AccountId Id, string Username, string Server, IList<AccessInput> Accesses) data)
        {
            var (id, username, server, accesses) = data;

            AccountInput.Id = id;
            AccountInput.Username = username;
            AccountInput.Server = server;
            AccountInput.SetAccesses(accesses);

            AccessInput.Clear();
        }

        [Reactive]
        private AccessInput? _selectedAccess;

        [ReactiveCommand(RunInBackground = true)]
        private void UpdateDatabase(AccountInput dto)
        {
            var account = new Account()
            {
                Username = dto.Username,
                Server = dto.Server,
                Accesses = [..dto.Accesses.Select(x => new Access()
                {
                    Username = x.Username,
                    Password = x.Password,
                    ProxyHost = x.ProxyHost,
                    ProxyPort = x.ProxyPort,
                    ProxyUsername = x.ProxyUsername,
                    ProxyPassword = x.ProxyPassword,
                })],
            };

            using var context = _contextFactory.CreateDbContext();
            context.Update(account);
            context.SaveChanges();

            var existingAccessIds = dto.Accesses.Select(a => a.Id.Value).ToHashSet();
            context.Accesses
                .Where(a => a.AccountId == account.Id && !existingAccessIds.Contains(a.Id))
                .ExecuteDelete();
        }
    }
}