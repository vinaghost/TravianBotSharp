using System.Text.Json;

using FluentValidation;

using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<AddAccountViewModel>]
    public partial class AddAccountViewModel : TabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;

        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly RxQueue _rxQueue;
        private readonly DefaultTemplatePathStore _defaultTemplatePathStore;

        public AddAccountViewModel(IValidator<AccessInput> accessInputValidator, DialogService dialogService, IValidator<AccountInput> accountInputValidator, WaitingOverlayViewModel waitingOverlayViewModel, RxQueue rxQueue, IDbContextFactory<AppDbContext> contextFactory, DefaultTemplatePathStore defaultTemplatePathStore) : base(dialogService)
        {
            _accessInputValidator = accessInputValidator;
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

            DeleteAccessCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(_ => SelectedAccess = null);
            AddAccountCommand.Where(x => x).ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(_ =>
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

            _waitingOverlayViewModel.Show("adding account");

            try
            {
                await AddToDatabaseCommand.Execute(AccountInput);
                _rxQueue.Enqueue(new AccountsModified());
            }
            finally
            {
                _waitingOverlayViewModel.Hide();
            }

            await _dialogService.SendMessage("Information", "Account created");

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

        [ReactiveCommand(RunInBackground = true)]
        private void AddToDatabase(AccountInput dto)
        {
            var defaultSettings = LoadAccountDefaultSettings();

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
                Settings = [],
            };

            foreach (var (setting, value) in defaultSettings)
            {
                account.Settings.Add(new AccountSetting
                {
                    Setting = setting,
                    Value = value,
                });
            }
            using var context = _contextFactory.CreateDbContext();
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