using FluentValidation;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsViewModel]
    public class EditAccountViewModel : AccountTabViewModelBase
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
        public ReactiveCommand<Unit, Unit> EditAccount { get; }

        public ReactiveCommand<AccountId, AccountDto> LoadAccount { get; }

        public EditAccountViewModel(IMediator mediator, IValidator<AccessInput> accessInputValidator, IValidator<AccountInput> accountInputValidator, WaitingOverlayViewModel waitingOverlayViewModel, IDialogService dialogService, IDbContextFactory<AppDbContext> contextFactory, IUseragentManager useragentManager)
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

        private void AddAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            AccountInput.Accesses.Add(AccessInput.Clone());
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

        private async Task EditAccountHandler()
        {
            var results = await _accountInputValidator.ValidateAsync(AccountInput);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return;
            }

            await _waitingOverlayViewModel.Show("editing account");

            var dto = AccountInput.ToDto();
            Update(dto);
            await _mediator.Publish(new AccountUpdated());
            await LoadAccount.Execute();

            await _waitingOverlayViewModel.Hide();

            _dialogService.ShowMessageBox("Information", "Edited account");
        }

        private AccountDto LoadAccountHandler(AccountId accountId)
        {
            var account = new GetAccount().Execute(AccountId, true);
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

        private void Update(AccountDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var account = dto.ToEntity();
            foreach (var access in account.Accesses.Where(access => string.IsNullOrWhiteSpace(access.Useragent)))
            {
                access.Useragent = _useragentManager.Get();
            }

            // Remove accesses not present in the DTO
            var existingAccessIds = dto.Accesses.Select(a => a.Id.Value).ToList();
            context.Accesses
                .Where(a => a.AccountId == account.Id && !existingAccessIds.Contains(a.Id))
                .ExecuteDelete();

            context.Update(account);
            context.SaveChanges();
        }
    }
}