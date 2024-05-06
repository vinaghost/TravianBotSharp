using FluentValidation;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterAsViewModel]
    public class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();

        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportCommand { get; }
        public ReactiveCommand<VillageId, Dictionary<VillageSettingEnums, int>> LoadSetting { get; }

        public VillageSettingViewModel(IMediator mediator, IValidator<VillageSettingInput> villageSettingInputValidator, IDialogService dialogService)
        {
            _mediator = mediator;
            _villageSettingInputValidator = villageSettingInputValidator;
            _dialogService = dialogService;

            SaveCommand = ReactiveCommand.CreateFromTask(SaveHandler);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportHandler);
            ImportCommand = ReactiveCommand.CreateFromTask(ImportHandler);
            LoadSetting = ReactiveCommand.Create<VillageId, Dictionary<VillageSettingEnums, int>>(LoadSettingHandler);

            LoadSetting.Subscribe(settings => VillageSettingInput.Set(settings));
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
            var result = _villageSettingInputValidator.Validate(VillageSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            var settings = VillageSettingInput.Get();
            new SetSettingCommand().Execute(VillageId, settings);
            await _mediator.Publish(new VillageSettingUpdated(AccountId, VillageId));

            _dialogService.ShowMessageBox("Information", "Settings saved");
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
            var result = _villageSettingInputValidator.Validate(VillageSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            settings = VillageSettingInput.Get();
            new SetSettingCommand().Execute(VillageId, settings);
            await _mediator.Publish(new VillageSettingUpdated(AccountId, VillageId));

            _dialogService.ShowMessageBox("Information", "Settings imported");
        }

        private async Task ExportHandler()
        {
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var settings = new GetSetting().Get(VillageId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }

        private Dictionary<VillageSettingEnums, int> LoadSettingHandler(VillageId villageId)
        {
            var settings = new GetSetting().Get(villageId);
            return settings;
        }
    }
}