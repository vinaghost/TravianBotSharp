using FluentValidation;
using MainCore.Commands.UI;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<EditAccountViewModel>]
    public class EditAccountViewModel : AccountTabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IDialogService _dialogService;
        public ReactiveCommand<Unit, Unit> AddAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccess { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccount { get; }

        public ReactiveCommand<AccountId, AccountDto> LoadAccount { get; }

        public EditAccountViewModel(IValidator<AccessInput> accessInputValidator, IDialogService dialogService)
        {
            _accessInputValidator = accessInputValidator;
            _dialogService = dialogService;

            AddAccess = ReactiveCommand.CreateFromTask(AddAccessHandler);
            EditAccess = ReactiveCommand.CreateFromTask(EditAccessHandler);
            DeleteAccess = ReactiveCommand.Create(DeleteAccessHandler);

            EditAccount = ReactiveCommand.CreateFromTask(EditAccountHandler);
            LoadAccount = ReactiveCommand.Create<AccountId, AccountDto>(LoadAccountHandler);

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccess.Subscribe(x => SelectedAccess = null);
            LoadAccount.Subscribe(SetAccount);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadAccount.Execute();
        }

        private async Task AddAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            AccountInput.Accesses.Add(AccessInput.Clone());
        }

        private async Task EditAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            AccessInput.CopyTo(SelectedAccess);
        }

        private void DeleteAccessHandler()
        {
            AccountInput.Accesses.Remove(SelectedAccess);
        }

        private async Task EditAccountHandler()
        {
            var updateAccountCommand = Locator.Current.GetService<UpdateAccountCommand>();
            await updateAccountCommand.Execute(AccountInput, default);
            await LoadAccount.Execute();
        }

        private AccountDto LoadAccountHandler(AccountId accountId)
        {
            var getAccount = Locator.Current.GetService<GetAccount>();
            var account = getAccount.Execute(AccountId, true);
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

        private AccessInput _selectedAccess;

        public AccessInput SelectedAccess
        {
            get => _selectedAccess;
            set => this.RaiseAndSetIfChanged(ref _selectedAccess, value);
        }
    }
}