using MainCore.Commands.UI;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<VillageSettingViewModel>]
    public partial class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();

        private readonly IDialogService _dialogService;

        public VillageSettingViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            LoadSettingCommand.Subscribe(VillageSettingInput.Set);
        }

        public async Task SettingRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadSettingCommand.Execute(villageId);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadSettingCommand.Execute(villageId);
        }

        [ReactiveCommand]
        private async Task Save()
        {
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, VillageId, VillageSettingInput, CancellationToken.None);
        }

        [ReactiveCommand]
        private async Task Import()
        {
            var path = await _dialogService.OpenFileDialog.Handle(Unit.Default);
            Dictionary<VillageSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<VillageSettingEnums, int>>(jsonString);
            }
            catch
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Invalid file."));
                return;
            }

            VillageSettingInput.Set(settings);
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, VillageId, VillageSettingInput, CancellationToken.None);
        }

        [ReactiveCommand]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var settings = getSetting.Get(VillageId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings exported"));
        }

        [ReactiveCommand]
        private static Dictionary<VillageSettingEnums, int> LoadSetting(VillageId villageId)
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var settings = getSetting.Get(villageId);
            return settings;
        }
    }
}