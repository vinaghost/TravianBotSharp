using FluentValidation;
using MainCore.Commands.UI.EditAccountViewModel;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

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

        public EditAccountViewModel(IValidator<AccessInput> accessInputValidator, IDialogService dialogService, IValidator<AccountInput> accountInputValidator, IWaitingOverlayViewModel waitingOverlayViewModel)
        {
            _accessInputValidator = accessInputValidator;
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccessCommand.Subscribe(x => SelectedAccess = null);
            LoadAccountCommand.Subscribe(SetAccount);
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadAccountCommand.Execute();
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
            var updateAccountCommand = Locator.Current.GetService<UpdateAccountCommand.Handler>();
            await updateAccountCommand.HandleAsync(new(AccountInput.ToDto()));
            await _waitingOverlayViewModel.Hide();
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Edited account"));

            await LoadAccountCommand.Execute();
        }

        [ReactiveCommand]
        private async Task<AccountDto> LoadAccount(AccountId accountId)
        {
            var getAcccountQuery = Locator.Current.GetService<GetAcccountQuery.Handler>();
            var account = await getAcccountQuery.HandleAsync(new(AccountId));
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
        private AccessInput _selectedAccess;
    }
}