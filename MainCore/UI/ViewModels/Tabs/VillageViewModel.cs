using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MainCore.UI.Enums;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class VillageViewModel : AccountTabViewModelBase
    {
        private readonly VillageTabStore _villageTabStore;

        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageViewModel(VillageTabStore villageTabStore, ITaskManager taskManager, IDialogService dialogService, IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _villageTabStore = villageTabStore;
            _dialogService = dialogService;

            _taskManager = taskManager;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            LoadCurrentCommand = ReactiveCommand.Create(LoadCurrentCommandHandler);
            LoadUnloadCommand = ReactiveCommand.CreateFromTask(LoadUnloadCommandHandler);
            LoadAllCommand = ReactiveCommand.CreateFromTask(LoadAllCommandHandler);

            var villageObservable = this.WhenAnyValue(x => x.Villages.SelectedItem);
            villageObservable.BindTo(_selectedItemStore, vm => vm.Village);
            villageObservable.Subscribe(x =>
            {
                var tabType = VillageTabType.Normal;
                if (x is null) tabType = VillageTabType.NoVillage;
                _villageTabStore.SetTabType(tabType);
            });
        }

        private void LoadCurrentCommandHandler()
        {
            if (!Villages.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No village selected");
                return;
            }
            var villageId = new VillageId(Villages.SelectedItemId);
            _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, villageId);
            _dialogService.ShowMessageBox("Information", $"Added update task");
            return;
        }

        private async Task LoadUnloadCommandHandler()
        {
            var villages = await Task.Run(() => _unitOfRepository.VillageRepository.GetMissingBuildingVillages(AccountId));
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, village);
            }
            _dialogService.ShowMessageBox("Information", $"Added update task");
            return;
        }

        private async Task LoadAllCommandHandler()
        {
            var villages = await Task.Run(() => _unitOfRepository.VillageRepository.Get(AccountId));
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, village);
            }
            _dialogService.ShowMessageBox("Information", $"Added update task");
            return;
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadVillageList(accountId);
        }

        public async Task VillageListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadVillageList(accountId);
        }

        private async Task LoadVillageList(AccountId accountId)
        {
            var villages = await Task.Run(() => _unitOfRepository.VillageRepository.GetItems(accountId));
            await Observable.Start(() =>
            {
                Villages.Load(villages);
            }, RxApp.MainThreadScheduler);
        }

        public VillageTabStore VillageTabStore => _villageTabStore;
        public ReactiveCommand<Unit, Unit> LoadCurrentCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadUnloadCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadAllCommand { get; }
    }
}