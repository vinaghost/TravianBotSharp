using MainCore.Infrasturecture.Extensions;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<VillageSettingViewModel>]
    public partial class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();

        private readonly IDialogService _dialogService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;
        private readonly ITaskManager _taskManager;

        public VillageSettingViewModel(IDialogService dialogService, IValidator<VillageSettingInput> villageSettingInputValidator, ICustomServiceScopeFactory serviceScopeFactory, IDbContextFactory<AppDbContext> contextFactory, ITaskManager taskManager)
        {
            _dialogService = dialogService;
            _villageSettingInputValidator = villageSettingInputValidator;
            _contextFactory = contextFactory;
            _taskManager = taskManager;

            LoadSettingCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(VillageSettingInput.Set);
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

        [ReactiveCommand(RunInBackground = true)]
        private async Task Save()
        {
            var result = await _villageSettingInputValidator.ValidateAsync(VillageSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            using var context = _contextFactory.CreateDbContext();
            var settings = VillageSettingInput.Get();
            context.SaveVillageSetting(VillageId, settings);
            context.TriggerTask(_taskManager, AccountId, VillageId, settings);

            await _dialogService.SendMessage("Information", "Settings saved.");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Import()
        {
            var path = await _dialogService.OpenFileDialog();
            Dictionary<VillageSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<VillageSettingEnums, int>>(jsonString)!;
            }
            catch
            {
                await _dialogService.SendMessage("Warning", "Invalid file.");
                return;
            }

            await Signal.Start(() => VillageSettingInput.Set(settings));
            var result = await _villageSettingInputValidator.ValidateAsync(VillageSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }
            using var context = _contextFactory.CreateDbContext();
            context.SaveVillageSetting(VillageId, settings);
            context.TriggerTask(_taskManager, AccountId, VillageId, settings);

            await _dialogService.SendMessage("Information", "Settings imported");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;

            using var context = _contextFactory.CreateDbContext();
            var settings = context.VillagesSetting
               .Where(x => x.VillageId == VillageId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.SendMessage("Information", "Settings exported");
        }

        [ReactiveCommand(RunInBackground = true)]
        private Dictionary<VillageSettingEnums, int> LoadSetting(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var settings = context.VillagesSetting
               .Where(x => x.VillageId == VillageId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}