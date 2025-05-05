using MainCore.Commands.UI.Misc;
using MainCore.Commands.UI.Villages.VillageSettingViewModel;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<VillageSettingViewModel>]
    public partial class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();

        private readonly IDialogService _dialogService;
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;

        public VillageSettingViewModel(IDialogService dialogService, IValidator<VillageSettingInput> villageSettingInputValidator)
        {
            _dialogService = dialogService;
            _villageSettingInputValidator = villageSettingInputValidator;

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
            var result = await _villageSettingInputValidator.ValidateAsync(VillageSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }
            var saveVillageSettingCommand = Locator.Current.GetService<SaveVillageSettingCommand.Handler>();
            await saveVillageSettingCommand.HandleAsync(new(AccountId, VillageId, VillageSettingInput.Get()));
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
            var result = await _villageSettingInputValidator.ValidateAsync(VillageSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }
            var saveVillageSettingCommand = Locator.Current.GetService<SaveVillageSettingCommand.Handler>();
            await saveVillageSettingCommand.HandleAsync(new(AccountId, VillageId, VillageSettingInput.Get()));
        }

        [ReactiveCommand]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;
            var getSettingQuery = Locator.Current.GetService<GetSettingQuery.Handler>();
            var settings = getSettingQuery.HandleAsync(new(VillageId));
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings exported"));
        }

        [ReactiveCommand]
        private static async Task<Dictionary<VillageSettingEnums, int>> LoadSetting(VillageId villageId)
        {
            var getSettingQuery = Locator.Current.GetService<GetSettingQuery.Handler>();
            var settings = await getSettingQuery.HandleAsync(new(villageId));
            return settings;
        }
    }
}