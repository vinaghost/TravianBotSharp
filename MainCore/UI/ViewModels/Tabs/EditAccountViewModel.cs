using MainCore.Commands.UI.EditAccountViewModel;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<EditAccountViewModel>]
    public partial class EditAccountViewModel : AccountTabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly IDialogService _dialogService;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        public EditAccountViewModel(IValidator<AccessInput> accessInputValidator, IDialogService dialogService, IValidator<AccountInput> accountInputValidator, IWaitingOverlayViewModel waitingOverlayViewModel, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _accessInputValidator = accessInputValidator;
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _serviceScopeFactory = serviceScopeFactory;

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccessCommand.Subscribe(x => SelectedAccess = null);
            LoadAccountCommand.Subscribe(SetAccount);
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
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
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
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
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

        [ReactiveCommand]
        private async Task EditAccount()
        {
            var results = await _accountInputValidator.ValidateAsync(AccountInput);

            if (!results.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", results.ToString()));
                return;
            }
            await _waitingOverlayViewModel.Show("editing account");

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var updateAccountCommand = scope.ServiceProvider.GetRequiredService<UpdateAccountCommand.Handler>();
            await updateAccountCommand.HandleAsync(new(AccountInput.ToDto()));
            await _waitingOverlayViewModel.Hide();
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Edited account"));

            await LoadAccountCommand.Execute(AccountId);
        }

        [ReactiveCommand]
        private AccountDto LoadAccount(AccountId accountId)
        {
            using var scope = _serviceScopeFactory.CreateScope(accountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var account = context.Accounts
               .Where(x => x.Id == accountId.Value)
               .Include(x => x.Accesses)
               .ToDto()
               .First();
            return account;
        }

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
    }
}
