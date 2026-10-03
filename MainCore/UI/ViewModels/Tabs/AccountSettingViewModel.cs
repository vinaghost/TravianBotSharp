using MainCore.Infrasturecture.Extensions;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<AccountSettingViewModel>]
    public partial class AccountSettingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();

        private readonly IDialogService _dialogService;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ITaskManager _taskManager;

        public AccountSettingViewModel(IDialogService dialogService, IValidator<AccountSettingInput> accountsettingInputValidator, IDbContextFactory<AppDbContext> contextFactory, ITaskManager taskManager)
        {
            _dialogService = dialogService;
            _accountsettingInputValidator = accountsettingInputValidator;
            _contextFactory = contextFactory;
            _taskManager = taskManager;

            LoadSettingsCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(AccountSettingInput.Set);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadSettingsCommand.Execute(accountId);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Save()
        {
            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            using var context = _contextFactory.CreateDbContext();
            var settings = AccountSettingInput.Get();
            context.SaveAccountSetting(AccountId, settings);
            context.TriggerTask(_taskManager, AccountId, settings);
            await _dialogService.SendMessage("Information", "Settings saved.");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Import()
        {
            var path = await _dialogService.OpenFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            Dictionary<AccountSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString)!;
            }
            catch
            {
                await _dialogService.SendMessage("Warning", "Invalid file.");
                return;
            }
            await Signal.Start(() => AccountSettingInput.Set(settings)).ObserveOn(RxSchedulers.MainThreadScheduler);

            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }
            using var context = _contextFactory.CreateDbContext();
            context.SaveAccountSetting(AccountId, settings);
            context.TriggerTask(_taskManager, AccountId, settings);
            await _dialogService.SendMessage("Information", "Settings imported.");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;

            using var context = _contextFactory.CreateDbContext();

            var settings = context.AccountsSetting
              .Where(x => x.AccountId == AccountId.Value)
              .ToDictionary(x => x.Setting, x => x.Value);

            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.SendMessage("Information", "Settings exported.");
        }

        [ReactiveCommand(RunInBackground = true)]
        private Dictionary<AccountSettingEnums, int> LoadSettings(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var settings = context.AccountsSetting
              .Where(x => x.AccountId == AccountId.Value)
              .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}