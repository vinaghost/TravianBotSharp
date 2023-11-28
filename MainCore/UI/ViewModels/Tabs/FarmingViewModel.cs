using FluentValidation;
using MainCore.Commands.UI.Farming;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class FarmingViewModel : AccountTabViewModelBase
    {
        public FarmListSettingInput FarmListSettingInput { get; } = new();
        private readonly IValidator<FarmListSettingInput> _farmListSettingInputValidator;
        public ListBoxItemViewModel FarmLists { get; } = new();

        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        public FarmingViewModel(IValidator<FarmListSettingInput> farmListSettingInputValidator, IDialogService dialogService, IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _farmListSettingInputValidator = farmListSettingInputValidator;

            _dialogService = dialogService;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            UpdateFarmList = ReactiveCommand.CreateFromTask(UpdateFarmListHandler);
            Start = ReactiveCommand.CreateFromTask(StartHandler);
            Stop = ReactiveCommand.CreateFromTask(StopHandler);

            Save = ReactiveCommand.CreateFromTask(SaveHandler);
            ActiveFarmList = ReactiveCommand.CreateFromTask(ActiveFarmListHandler);
        }

        public async Task FarmListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadFarmLists(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadFarmLists(accountId);
            await LoadSettings(accountId);
        }

        private async Task UpdateFarmListHandler()
        {
            await _mediator.Send(new UpdateFarmListCommand(AccountId));
        }

        private async Task StartHandler()
        {
            if (!FarmListSettingInput.UseStartAllButton)
            {
                var count = _unitOfRepository.FarmRepository.CountActive(AccountId);
                if (count == 0)
                {
                    _dialogService.ShowMessageBox("Information", "There is no active farm or use start all button is disable");
                    return;
                }
            }
            await _mediator.Send(new StartFarmListCommand(AccountId));
        }

        private async Task StopHandler()
        {
            await _mediator.Send(new StopFarmListCommand(AccountId));
        }

        private async Task SaveHandler()
        {
            var result = _farmListSettingInputValidator.Validate(FarmListSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var settings = FarmListSettingInput.Get();
            await _mediator.Send(new SaveFarmListSettingsCommand(AccountId, settings));
        }

        private async Task ActiveFarmListHandler()
        {
            var selectedFarmList = FarmLists.SelectedItem;
            if (selectedFarmList is null)
            {
                _dialogService.ShowMessageBox("Warning", "No farm list selected");
                return;
            }

            var farmId = new FarmId(selectedFarmList.Id);
            await _mediator.Send(new ActiveFarmListCommand(AccountId, farmId));
        }

        private async Task LoadSettings(AccountId accountId)
        {
            var settings = await Observable.Start(() =>
            {
                return _unitOfRepository.AccountSettingRepository.Get(accountId);
            }, RxApp.TaskpoolScheduler);
            await Observable.Start(() =>
                {
                    FarmListSettingInput.Set(settings);
                }, RxApp.MainThreadScheduler);
        }

        private async Task LoadFarmLists(AccountId accountId)
        {
            var farmLists = await Observable.Start(() =>
            {
                return _unitOfRepository.FarmRepository.GetItems(accountId);
            }, RxApp.TaskpoolScheduler);

            await Observable.Start(() =>
            {
                FarmLists.Load(farmLists);
            }, RxApp.MainThreadScheduler);
        }

        public ReactiveCommand<Unit, Unit> UpdateFarmList { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> ActiveFarmList { get; }
        public ReactiveCommand<Unit, Unit> Start { get; }
        public ReactiveCommand<Unit, Unit> Stop { get; }
    }
}