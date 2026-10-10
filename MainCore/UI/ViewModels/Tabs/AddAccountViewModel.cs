using System.Text.Json;

using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.ViewModels.Tabs
{
    using FluentValidation;

    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<AddAccountViewModel>]
    public partial class AddAccountViewModel : TabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;

        private readonly DialogService _dialogService;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly RxQueue _rxQueue;
        private readonly DefaultTemplatePathStore _defaultTemplatePathStore;

        public AddAccountViewModel(IValidator<AccessInput> accessInputValidator, DialogService dialogService, IValidator<AccountInput> accountInputValidator, IWaitingOverlayViewModel waitingOverlayViewModel, RxQueue rxQueue, IDbContextFactory<AppDbContext> contextFactory, DefaultTemplatePathStore defaultTemplatePathStore)
        {
            _accessInputValidator = accessInputValidator;
            _dialogService = dialogService;
            _accountInputValidator = accountInputValidator;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _rxQueue = rxQueue;
            _contextFactory = contextFactory;
            _defaultTemplatePathStore = defaultTemplatePathStore;

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
            var defaultSettings = LoadAccountDefaultSettings();

            account.Settings = [];
            foreach (var (setting, value) in defaultSettings)
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

        private Dictionary<AccountSettingEnums, int> LoadAccountDefaultSettings()
        {
            var settings = AppDbContext.AccountDefaultSettings
                .ToDictionary(x => x.Key, x => x.Value);

            var path = _defaultTemplatePathStore.Get().AccountSettingsPath;
            if (string.IsNullOrWhiteSpace(path)) return settings;
            if (!File.Exists(path)) return settings;

            try
            {
                var jsonString = File.ReadAllText(path);
                var importedSettings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString);
                if (importedSettings is null) return settings;

                foreach (var (setting, value) in importedSettings)
                {
                    if (setting == AccountSettingEnums.Tribe) continue;
                    settings[setting] = value;
                }
            }
            catch
            {
                return settings;
            }

            return settings;
        }
    }
}