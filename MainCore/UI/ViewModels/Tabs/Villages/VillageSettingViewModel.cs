﻿using MainCore.Commands.UI.Misc;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<VillageSettingViewModel>]
    public partial class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();

        private readonly IDialogService _dialogService;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;

        public VillageSettingViewModel(IDialogService dialogService, IValidator<VillageSettingInput> villageSettingInputValidator, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _dialogService = dialogService;
            _villageSettingInputValidator = villageSettingInputValidator;
            _serviceScopeFactory = serviceScopeFactory;

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

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var saveVillageSettingCommand = scope.ServiceProvider.GetRequiredService<SaveVillageSettingCommand.Handler>();
            await saveVillageSettingCommand.HandleAsync(new(AccountId, VillageId, VillageSettingInput.Get()));

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings saved."));
        }

        [ReactiveCommand]
        private async Task Import()
        {
            var path = await _dialogService.OpenFileDialog.Handle(Unit.Default);
            Dictionary<VillageSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<VillageSettingEnums, int>>(jsonString)!;
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

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var saveVillageSettingCommand = scope.ServiceProvider.GetRequiredService<SaveVillageSettingCommand.Handler>();
            await saveVillageSettingCommand.HandleAsync(new(AccountId, VillageId, VillageSettingInput.Get()));

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings imported"));
        }

        [ReactiveCommand]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var settings = context.VillagesSetting
               .Where(x => x.VillageId == VillageId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings exported"));
        }

        [ReactiveCommand]
        private Dictionary<VillageSettingEnums, int> LoadSetting(VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var settings = context.VillagesSetting
               .Where(x => x.VillageId == VillageId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}