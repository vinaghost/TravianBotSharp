using MainCore.Commands.UI;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<AccountSettingViewModel>]
    public partial class AccountSettingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();
        private readonly IDialogService _dialogService;

        public AccountSettingViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            LoadSettingsCommand.Subscribe(AccountSettingInput.Set);
        }

        public async Task SettingRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadSettingsCommand.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadSettingsCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task Save()
        {
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, AccountSettingInput, CancellationToken.None);
        }

        [ReactiveCommand]
        private async Task Import()
        {
            var path = await _dialogService.OpenFileDialog.Handle(Unit.Default);
            Dictionary<AccountSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString);
            }
            catch
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Invalid file."));
                return;
            }

            AccountSettingInput.Set(settings);
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, AccountSettingInput, CancellationToken.None);
        }

        [ReactiveCommand]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;

            var getSetting = Locator.Current.GetService<IGetSetting>();
            var settings = getSetting.Get(AccountId);

            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings exported."));
        }

        [ReactiveCommand]
        private static Dictionary<AccountSettingEnums, int> LoadSettings(AccountId accountId)
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var settings = getSetting.Get(accountId);
            return settings;
        }
    }
}