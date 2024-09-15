using FluentValidation;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton(Registration = RegistrationStrategy.Self)]
    public class AccountSettingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();

        private readonly IMediator _mediator;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDialogService _dialogService;
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }

        public ReactiveCommand<AccountId, Dictionary<AccountSettingEnums, int>> LoadSettings { get; }

        public AccountSettingViewModel(IMediator mediator, IValidator<AccountSettingInput> accountsettingInputValidator, IDialogService dialogService)
        {
            _mediator = mediator;
            _accountsettingInputValidator = accountsettingInputValidator;
            _dialogService = dialogService;

            Save = ReactiveCommand.CreateFromTask(SaveHandler);
            Export = ReactiveCommand.CreateFromTask(ExportHandler);
            Import = ReactiveCommand.CreateFromTask(ImportHandler);
            LoadSettings = ReactiveCommand.Create<AccountId, Dictionary<AccountSettingEnums, int>>(LoadSettingsHandler);

            LoadSettings.Subscribe(x => AccountSettingInput.Set(x));
        }

        public async Task SettingRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadSettings.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadSettings.Execute(accountId);
        }

        private async Task SaveHandler()
        {
            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var settings = AccountSettingInput.Get();
            new SetSettingCommand().Execute(AccountId, settings);
            await _mediator.Publish(new AccountSettingUpdated(AccountId));
            _dialogService.ShowMessageBox("Information", message: "Settings saved");
        }

        private async Task ImportHandler()
        {
            var path = _dialogService.OpenFileDialog();
            Dictionary<AccountSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString);
            }
            catch
            {
                _dialogService.ShowMessageBox("Warning", "Invalid file.");
                return;
            }

            AccountSettingInput.Set(settings);
            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            settings = AccountSettingInput.Get();
            new SetSettingCommand().Execute(AccountId, settings);
            await _mediator.Publish(new AccountSettingUpdated(AccountId));

            _dialogService.ShowMessageBox("Information", "Settings imported");
        }

        private async Task ExportHandler()
        {
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var settings = new GetSetting().Get(AccountId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }

        private static Dictionary<AccountSettingEnums, int> LoadSettingsHandler(AccountId accountId)
        {
            var settings = new GetSetting().Get(accountId);
            return settings;
        }
    }
}