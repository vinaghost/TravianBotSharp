using MainCore.Commands.UI;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<VillageSettingViewModel>]
    public class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();

        private readonly IDialogService _dialogService;
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportCommand { get; }
        public ReactiveCommand<VillageId, Dictionary<VillageSettingEnums, int>> LoadSetting { get; }

        public VillageSettingViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            SaveCommand = ReactiveCommand.CreateFromTask(SaveHandler);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportHandler);
            ImportCommand = ReactiveCommand.CreateFromTask(ImportHandler);
            LoadSetting = ReactiveCommand.Create<VillageId, Dictionary<VillageSettingEnums, int>>(LoadSettingHandler);

            LoadSetting.Subscribe(VillageSettingInput.Set);
        }

        public async Task SettingRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadSetting.Execute(villageId);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadSetting.Execute(villageId);
        }

        private async Task SaveHandler()
        {
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, VillageId, VillageSettingInput, CancellationToken.None);
        }

        private async Task ImportHandler()
        {
            var path = _dialogService.OpenFileDialog();
            Dictionary<VillageSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<VillageSettingEnums, int>>(jsonString);
            }
            catch
            {
                _dialogService.ShowMessageBox("Warning", "Invalid file.");
                return;
            }

            VillageSettingInput.Set(settings);
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, VillageId, VillageSettingInput, CancellationToken.None);
        }

        private async Task ExportHandler()
        {
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var settings = getSetting.Get(VillageId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }

        private static Dictionary<VillageSettingEnums, int> LoadSettingHandler(VillageId villageId)
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var settings = getSetting.Get(villageId);
            return settings;
        }
    }
}