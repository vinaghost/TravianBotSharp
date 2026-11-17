using System.Text.Json;

using FluentValidation;

using MainCore.Features.ClaimDailyQuest;
using MainCore.Features.SetHeroPoint;
using MainCore.Features.StartAdventure;
using MainCore.Features.Update;
using MainCore.UI.Models.Input;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<AccountSettingViewModel>]
    public partial class AccountSettingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();

        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly TaskManager _taskManager;

        public AccountSettingViewModel(DialogService dialogService, IValidator<AccountSettingInput> accountsettingInputValidator, IDbContextFactory<AppDbContext> contextFactory, TaskManager taskManager, SelectedItemStore selectedItemStore) : base(dialogService, selectedItemStore)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _contextFactory = contextFactory;
            _taskManager = taskManager;

            LoadSettingsCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(AccountSettingInput.Set);
            ImportCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(AccountSettingInput.Set);
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

            var settings = AccountSettingInput.Get();

            SaveSetting(AccountId, settings);
            TriggerTask(AccountId, settings);
            await _dialogService.SendMessage("Information", "Settings saved.");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task<Dictionary<AccountSettingEnums, int>> Import()
        {
            var path = await _dialogService.OpenFileDialog();
            if (string.IsNullOrEmpty(path)) return [];
            Dictionary<AccountSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString)!;
            }
            catch (Exception ex)
            {
                await _dialogService.SendMessage("Error", $"Cannot import settings. [{ex.Message}]");
                return [];
            }

            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return [];
            }

            SaveSetting(AccountId, settings);
            TriggerTask(AccountId, settings);
            await _dialogService.SendMessage("Information", "Settings imported.");

            return settings;
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;

            var settings = GetSettings(AccountId);

            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.SendMessage("Information", "Settings exported.");
        }

        [ReactiveCommand(RunInBackground = true)]
        private Dictionary<AccountSettingEnums, int> LoadSettings(AccountId accountId)
        {
            return GetSettings(accountId);
        }

        private Dictionary<AccountSettingEnums, int> GetSettings(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var settings = context.AccountsSetting
              .Where(x => x.AccountId == accountId.Value)
              .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }

        private void TriggerTask(AccountId accountId, Dictionary<AccountSettingEnums, int> settings)
        {
            if (settings.Count == 0) return;
            using var context = _contextFactory.CreateDbContext();

            if (settings.ContainsKey(AccountSettingEnums.EnableAutoLoadVillageBuilding))
            {
                if (settings[AccountSettingEnums.EnableAutoLoadVillageBuilding] == 1)
                {
                    var villages = context.Villages
                        .Where(x => x.AccountId == accountId.Value)
                        .Where(x => x.Buildings.Count < 40)
                        .Select(x => new VillageId(x.Id));

                    foreach (var village in villages)
                    {
                        _taskManager.Add(new UpdateBuildingTask.Task(accountId, village));
                    }
                }
                else
                {
                    var villages = context.Villages
                        .Where(x => x.AccountId == accountId.Value)
                        .Select(x => new VillageId(x.Id))
                        .ToList();
                    foreach (var village in villages)
                    {
                        _taskManager.Remove<UpdateBuildingTask.Task>(accountId, village);
                    }
                }
            }

            if (settings.ContainsKey(AccountSettingEnums.EnableAutoStartAdventure))
            {
                if (settings[AccountSettingEnums.EnableAutoStartAdventure] == 1)
                {
                    if (context.IsStartAdventureEnable(accountId))
                    {
                        _taskManager.Add(new StartAdventureTask.Task(accountId));
                    }
                }
                else
                {
                    _taskManager.Remove<StartAdventureTask.Task>(accountId);
                }
            }

            if (settings.ContainsKey(AccountSettingEnums.EnableAutoClaimDailyQuest))
            {
                if (settings[AccountSettingEnums.EnableAutoClaimDailyQuest] == 1)
                {
                    if (context.IsDailyQuestEnable(accountId))
                    {
                        _taskManager.Add(new ClaimDailyQuestTask.Task(accountId));
                    }
                }
                else
                {
                    _taskManager.Remove<ClaimDailyQuestTask.Task>(accountId);
                }
            }

            if (settings.ContainsKey(AccountSettingEnums.EnableAutoSetHeroPoint))
            {
                if (settings[AccountSettingEnums.EnableAutoSetHeroPoint] == 1)
                {
                    if (context.IsSetHeroPointEnable(accountId))
                    {
                        _taskManager.Add(new SetHeroPointTask.Task(accountId));
                    }
                }
                else
                {
                    _taskManager.Remove<SetHeroPointTask.Task>(accountId);
                }
            }
        }

        private void SaveSetting(AccountId accountId, Dictionary<AccountSettingEnums, int> settings)
        {
            settings.Remove(AccountSettingEnums.Tribe);

            if (settings.Count == 0) return;
            using var context = _contextFactory.CreateDbContext();
            foreach (var setting in settings)
            {
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }
    }
}