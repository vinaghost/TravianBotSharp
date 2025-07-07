﻿using MainCore.Commands.UI.AccountSettingViewModel;
using MainCore.Commands.UI.Misc;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<AccountSettingViewModel>]
    public partial class AccountSettingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();

        private readonly IDialogService _dialogService;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        public AccountSettingViewModel(IDialogService dialogService, IValidator<AccountSettingInput> accountsettingInputValidator, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _dialogService = dialogService;
            _accountsettingInputValidator = accountsettingInputValidator;
            _serviceScopeFactory = serviceScopeFactory;

            LoadSettingsCommand.Subscribe(AccountSettingInput.Set);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadSettingsCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task Save()
        {
            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var saveAccountSettingCommand = scope.ServiceProvider.GetRequiredService<SaveAccountSettingCommand.Handler>();
            await saveAccountSettingCommand.HandleAsync(new(AccountId, AccountSettingInput.Get()));

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings saved."));
        }

        [ReactiveCommand]
        private async Task Import()
        {
            var path = await _dialogService.OpenFileDialog.Handle(Unit.Default);
            Dictionary<AccountSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                settings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString)!;
            }
            catch
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Invalid file."));
                return;
            }

            AccountSettingInput.Set(settings);
            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var saveAccountSettingCommand = scope.ServiceProvider.GetRequiredService<SaveAccountSettingCommand.Handler>();
            await saveAccountSettingCommand.HandleAsync(new(AccountId, AccountSettingInput.Get()));

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings imported."));
        }

        [ReactiveCommand]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getSettingQuery = scope.ServiceProvider.GetRequiredService<GetSettingQuery.Handler>();
            var settings = getSettingQuery.HandleAsync(new(AccountId));

            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings exported."));
        }

        [ReactiveCommand]
        private async Task<Dictionary<AccountSettingEnums, int>> LoadSettings(AccountId accountId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getSettingQuery = scope.ServiceProvider.GetRequiredService<GetSettingQuery.Handler>();
            var settings = await getSettingQuery.HandleAsync(new(accountId));
            return settings;
        }
    }
}