using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Extensions;

    [RegisterSingleton<EditAccountViewModel>]
    public partial class EditAccountViewModel : AccountTabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly IDialogService _dialogService;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IRxQueue _rxQueue;

        public EditAccountViewModel(IValidator<AccessInput> accessInputValidator, IDialogService dialogService, IValidator<AccountInput> accountInputValidator, IWaitingOverlayViewModel waitingOverlayViewModel, IDbContextFactory<AppDbContext> contextFactory, IRxQueue rxQueue)
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
            EditAccountCommand.InvokeCommand(LoadAccountCommand);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadAccountCommand.Execute(accountId).ToHotTask();
        }

        [ReactiveCommand]
        private async Task AddAccess()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", result.ToString());
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
                await _dialogService.SendMessage("Error", result.ToString());
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
                await _dialogService.SendMessage("Error", results.ToString());
                return;
            }
            await _waitingOverlayViewModel.Show("editing account");

            UpdateDatabase(AccountInput.ToDto());
            _rxQueue.Enqueue(new AccountsModified());

            await _waitingOverlayViewModel.Hide();
            await _dialogService.SendMessage("Information", "Edited account");
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