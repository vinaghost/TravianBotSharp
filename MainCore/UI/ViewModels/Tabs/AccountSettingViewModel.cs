using FluentValidation;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using System.Text.Json;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class AccountSettingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;

        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }

        public AccountSettingViewModel(IValidator<AccountSettingInput> accountsettingInputValidator, IDialogService dialogService, IUnitOfRepository unitOfRepository, IMediator mediator)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;

            Save = ReactiveCommand.CreateFromTask(SaveHandler);
            Export = ReactiveCommand.CreateFromTask(ExportHandler);
            Import = ReactiveCommand.CreateFromTask(ImportHandler);
        }

        public async Task SettingRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadSettings(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadSettings(accountId);
        }

        private async Task SaveHandler()
        {
            var result = _accountsettingInputValidator.Validate(AccountSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            var settings = AccountSettingInput.Get();
            _unitOfRepository.AccountSettingRepository.Update(AccountId, settings);
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
            var result = _accountsettingInputValidator.Validate(AccountSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            settings = AccountSettingInput.Get();
            _unitOfRepository.AccountSettingRepository.Update(AccountId, settings);
            await _mediator.Publish(new AccountSettingUpdated(AccountId));

            _dialogService.ShowMessageBox("Information", "Settings imported");
        }

        private async Task ExportHandler()
        {
            var path = _dialogService.SaveFileDialog();
            var settings = _unitOfRepository.AccountSettingRepository.Get(AccountId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            _dialogService.ShowMessageBox("Settings exported", "Information");
        }

        private async Task LoadSettings(AccountId accountId)
        {
            var settings = await Observable.Start(() =>
            {
                var settings = _unitOfRepository.AccountSettingRepository.Get(accountId);
                return settings;
            }, RxApp.TaskpoolScheduler);

            await Observable.Start(() =>
            {
                AccountSettingInput.Set(settings);
            }, RxApp.MainThreadScheduler);
        }
    }
}