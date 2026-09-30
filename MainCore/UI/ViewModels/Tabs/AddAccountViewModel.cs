using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.UI.ViewModels.Tabs
{
    using Humanizer;
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Concurrency;
    using ReactiveUI.Primitives.Disposables;
    using ReactiveUI.Primitives.Extensions;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<AddAccountViewModel>]
    public partial class AddAccountViewModel : TabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;

        private readonly IDialogService _dialogService;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IRxQueue _rxQueue;

        public AddAccountViewModel(IValidator<AccessInput> accessInputValidator, IDialogService dialogService, IValidator<AccountInput> accountInputValidator, IWaitingOverlayViewModel waitingOverlayViewModel, IRxQueue rxQueue, IDbContextFactory<AppDbContext> contextFactory)
        {
            _accessInputValidator = accessInputValidator;
            _dialogService = dialogService;
            _accountInputValidator = accountInputValidator;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _rxQueue = rxQueue;
            _contextFactory = contextFactory;

            Init();
        }

        private void Init()
        {
            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccessCommand.Subscribe(x => SelectedAccess = null);
            AddAccountCommand.Where(x => x).Subscribe(x =>
            {
                AccountInput.Clear();
                AccessInput.Clear();
            });
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

        [ReactiveCommand]
        private async Task<bool> AddAccount()
        {
            var result = await _accountInputValidator.ValidateAsync(AccountInput);

            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return false;
            }

            if (IsDuplicated(AccountInput))
            {
                await _dialogService.SendMessage("Error", "Account is duplicated");
                return false;
            }

            await _waitingOverlayViewModel.Show("adding account");

            await Signal.Start(() => UpdateDatabase(AccountInput.ToDto()), RxSchedulers.TaskpoolScheduler);
            _rxQueue.Enqueue(new AccountsModified());

            await _waitingOverlayViewModel.Hide();

            await _dialogService.SendMessage("Information", "Added account");
            return true;
        }

        [Reactive]
        private AccessInput? _selectedAccess;

        private bool IsDuplicated(AccountInput input)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Accounts
                .Any(x => x.Username == input.Username && x.Server == input.Server);
        }

        private void UpdateDatabase(AccountDto dto)
        {
            using var context = _contextFactory.CreateDbContext();
            var account = dto.ToEntity();

            account.Settings = [];
            foreach (var (setting, value) in AppDbContext.AccountDefaultSettings)
            {
                account.Settings.Add(new AccountSetting
                {
                    Setting = setting,
                    Value = value,
                });
            }
            context.Add(account);
            context.SaveChanges();
        }
    }
}