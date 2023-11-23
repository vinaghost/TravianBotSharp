using FluentValidation;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
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

        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        public FarmingViewModel(IValidator<FarmListSettingInput> farmListSettingInputValidator, ITaskManager taskManager, IDialogService dialogService, IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _farmListSettingInputValidator = farmListSettingInputValidator;

            _taskManager = taskManager;
            _dialogService = dialogService;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            UpdateFarmListCommand = ReactiveCommand.Create(UpdateFarmListCommandHandler);
            SaveCommand = ReactiveCommand.CreateFromTask(SaveCommandHandler);
            ActiveFarmListCommand = ReactiveCommand.CreateFromTask(ActiveFarmListCommandHandler);
            StartCommand = ReactiveCommand.CreateFromTask(StartCommandHandler);
            StopCommand = ReactiveCommand.Create(StopCommandHandler);
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

        private void UpdateFarmListCommandHandler()
        {
            _taskManager.AddOrUpdate<UpdateFarmListTask>(AccountId);
            _dialogService.ShowMessageBox("Information", "Added update farm list task");
        }

        private async Task StartCommandHandler()
        {
            if (!FarmListSettingInput.UseStartAllButton)
            {
                var count = await Task.Run(() => _unitOfRepository.FarmRepository.CountActive(AccountId));
                if (count == 0)
                {
                    _dialogService.ShowMessageBox("Information", "There is no active farm or use start all button is disable");
                    return;
                }
            }

            _taskManager.AddOrUpdate<StartFarmListTask>(AccountId);

            _dialogService.ShowMessageBox("Information", "Added start farm list task");
        }

        private void StopCommandHandler()
        {
            var task = _taskManager.Get<StartFarmListTask>(AccountId);

            if (task is not null)
            {
                _taskManager.Remove(AccountId, task);
            }
            _dialogService.ShowMessageBox("Information", "Removed start farm list task");
        }

        private async Task SaveCommandHandler()
        {
            var result = _farmListSettingInputValidator.Validate(FarmListSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            var settings = FarmListSettingInput.Get();
            await Task.Run(() => _unitOfRepository.AccountSettingRepository.Update(AccountId, settings));
            await _mediator.Publish(new AccountSettingUpdated(AccountId));

            _dialogService.ShowMessageBox("Information", "Settings saved");
        }

        private async Task ActiveFarmListCommandHandler()
        {
            var SelectedFarmList = FarmLists.SelectedItem;
            if (FarmLists.SelectedItem is null)
            {
                _dialogService.ShowMessageBox("Warning", "No farm list selected");
                return;
            }
            await Task.Run(() => _unitOfRepository.FarmRepository.ChangeActive(new FarmId(SelectedFarmList.Id)));
            await _mediator.Publish(new FarmListUpdated(AccountId));
        }

        private async Task LoadSettings(AccountId accountId)
        {
            var settings = await Task.Run(() => _unitOfRepository.AccountSettingRepository.Get(accountId));
            await Observable.Start(() =>
            {
                FarmListSettingInput.Set(settings);
            }, RxApp.MainThreadScheduler);
        }

        private async Task LoadFarmLists(AccountId accountId)
        {
            var farmLists = await Task.Run(() => _unitOfRepository.FarmRepository.GetItems(accountId));
            await Observable.Start(() =>
            {
                FarmLists.Load(farmLists);
            }, RxApp.MainThreadScheduler);
        }

        public ReactiveCommand<Unit, Unit> UpdateFarmListCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ActiveFarmListCommand { get; }
        public ReactiveCommand<Unit, Unit> StartCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
    }
}