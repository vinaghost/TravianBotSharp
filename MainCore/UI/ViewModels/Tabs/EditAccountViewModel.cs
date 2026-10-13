using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.ViewModels.Tabs
{
    using FluentValidation;

    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<EditAccountViewModel>]
    public partial class EditAccountViewModel : AccountTabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly DialogService _dialogService;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly RxQueue _rxQueue;

        public EditAccountViewModel(IValidator<AccessInput> accessInputValidator, DialogService dialogService, IValidator<AccountInput> accountInputValidator, IWaitingOverlayViewModel waitingOverlayViewModel, IDbContextFactory<AppDbContext> contextFactory, RxQueue rxQueue)
        {
            _accessInputValidator = accessInputValidator;
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;
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

            DeleteAccessCommand.Subscribe(_ => SelectedAccess = null);

            LoadAccountCommand.InvokeCommand(SetAccountCommand);
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

            AccountInput.Accesses.Add(AccessInput.Clone());
        }

        [ReactiveCommand]
        private async Task EditAccess()
        {
            if (SelectedAccess is null) return;
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            AccessInput.CopyTo(SelectedAccess);
        }

        [ReactiveCommand]
        private void DeleteAccess()
        {
            if (SelectedAccess is null) return;
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
            await _waitingOverlayViewModel.Show("editing account");

            UpdateDatabase(AccountInput.ToDto());
            _rxQueue.Enqueue(new AccountsModified());

            await _waitingOverlayViewModel.Hide();
            await _dialogService.SendMessage("Information", "Edited account");

            await LoadAccountCommand.Execute(AccountId);
        }

        [ReactiveCommand(RunInBackground = true)]
        private AccountDto LoadAccount(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var account = context.Accounts
               .Where(x => x.Id == accountId.Value)
               .Include(x => x.Accesses)
               .ToDto()
               .First();
            return account;
        }

        [ReactiveCommand]
        private void SetAccount(AccountDto account)
        {
            AccountInput.Id = account.Id;
            AccountInput.Username = account.Username;
            AccountInput.Server = account.Server;
            AccountInput.SetAccesses(account.Accesses.Select(x => x.ToInput()));

            AccessInput.Clear();
        }

        [Reactive]
        private AccessInput? _selectedAccess;

        private void UpdateDatabase(AccountDto dto)
        {
            var account = dto.ToEntity();

            using var context = _contextFactory.CreateDbContext();
            var existingAccessIds = dto.Accesses.Select(a => a.Id.Value).ToList();

            context.Accesses
                .Where(a => a.AccountId == account.Id && !existingAccessIds.Contains(a.Id))
                .ExecuteDelete();

            context.Update(account);
            context.SaveChanges();
        }
    }
}