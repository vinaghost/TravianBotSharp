using FluentValidation;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton(Registration = RegistrationStrategy.Self)]
    public class AddAccountViewModel : TabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IMediator _mediator;

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;

        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDialogService _dialogService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IUseragentManager _useragentManager;
        public ReactiveCommand<Unit, Unit> AddAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccess { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccess { get; }
        public ReactiveCommand<Unit, Unit> AddAccount { get; }

        public AddAccountViewModel(IMediator mediator, IValidator<AccessInput> accessInputValidator, IValidator<AccountInput> accountInputValidator, WaitingOverlayViewModel waitingOverlayViewModel, IDialogService dialogService, IDbContextFactory<AppDbContext> contextFactory, IUseragentManager useragentManager)
        {
            _mediator = mediator;
            _accessInputValidator = accessInputValidator;
            _accountInputValidator = accountInputValidator;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _dialogService = dialogService;
            _contextFactory = contextFactory;
            _useragentManager = useragentManager;

            AddAccess = ReactiveCommand.Create(AddAccessHandler);
            EditAccess = ReactiveCommand.Create(EditAccessHandler);
            DeleteAccess = ReactiveCommand.Create(DeleteAccessHandler);
            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccess.Subscribe(x => SelectedAccess = null);
            AddAccount.Subscribe(x =>
            {
                AccountInput.Clear();
                AccessInput.Clear();
            });
        }

        private void AddAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            if (AccountInput.Accesses.Count == 0)
            {
                AccountInput.Accesses.Add(AccessInput.Clone());
            }
            else
            {
                _dialogService.ShowMessageBox("Error", "Only one access is allowed because new rule. Please check TBS's discord server");
            }
        }

        private void EditAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            AccessInput.CopyTo(SelectedAccess);
        }

        private void DeleteAccessHandler()
        {
            AccountInput.Accesses.Remove(SelectedAccess);
        }

        private async Task AddAccountHandler()
        {
            var results = await _accountInputValidator.ValidateAsync(AccountInput);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return;
            }
            await _waitingOverlayViewModel.Show("adding account");

            var dto = AccountInput.ToDto();
            var success = Add(dto);
            if (success) await _mediator.Publish(new AccountUpdated());

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", success ? "Added account" : "Account is duplicated");
        }

        private bool Add(AccountDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var isExist = context.Accounts
                .Where(x => x.Username == dto.Username)
                .Where(x => x.Server == dto.Server)
                .Any();

            if (isExist) return false;

            var account = dto.ToEntity();
            foreach (var access in account.Accesses.Where(access => string.IsNullOrEmpty(access.Useragent)))
            {
                access.Useragent = _useragentManager.Get();
            }

            context.Add(account);
            context.SaveChanges();
            context.FillAccountSettings(new(account.Id));
            return true;
        }

        private AccessInput _selectedAccess;

        public AccessInput SelectedAccess
        {
            get => _selectedAccess;
            set => this.RaiseAndSetIfChanged(ref _selectedAccess, value);
        }
    }
}