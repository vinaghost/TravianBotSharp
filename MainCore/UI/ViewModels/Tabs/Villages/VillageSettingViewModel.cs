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

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;

        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportCommand { get; }

        public VillageSettingViewModel(IValidator<VillageSettingInput> villageSettingInputValidator, IDialogService dialogService, IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _villageSettingInputValidator = villageSettingInputValidator;
            _dialogService = dialogService;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            SaveCommand = ReactiveCommand.CreateFromTask(SaveHandler);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportHandler);
            ImportCommand = ReactiveCommand.CreateFromTask(ImportHandler);
        }

        public async Task SettingRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadSettings(villageId);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadSettings(villageId);
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
            await Task.Run(() => _unitOfRepository.VillageSettingRepository.Update(VillageId, settings));
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
            await Task.Run(() => _unitOfRepository.VillageSettingRepository.Update(VillageId, settings));
            await _mediator.Publish(new VillageSettingUpdated(AccountId, VillageId));

            _dialogService.ShowMessageBox("Information", "Settings imported");
        }

        private async Task ExportHandler()
        {
            var path = _dialogService.SaveFileDialog();
            var settings = await Task.Run(() => _unitOfRepository.VillageSettingRepository.Get(VillageId));
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }

        private async Task LoadSettings(VillageId villageId)
        {
            var settings = await Task.Run(() => _unitOfRepository.VillageSettingRepository.Get(villageId));
            await Observable.Start(() =>
            {
                VillageSettingInput.Set(settings);
            }, RxApp.MainThreadScheduler);
        }
    }
}