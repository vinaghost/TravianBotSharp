using MainCore.Commands.UI.Village;
using MainCore.UI.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class VillageViewModel : AccountTabViewModelBase
    {
        private readonly VillageTabStore _villageTabStore;

        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageTabStore VillageTabStore => _villageTabStore;
        public ReactiveCommand<Unit, Unit> LoadCurrent { get; }
        public ReactiveCommand<Unit, Unit> LoadUnload { get; }
        public ReactiveCommand<Unit, Unit> LoadAll { get; }
        public ReactiveCommand<AccountId, List<ListBoxItem>> LoadVillage { get; }

        public VillageViewModel(VillageTabStore villageTabStore, IMediator mediator, UnitOfRepository unitOfRepository)
        {
            _villageTabStore = villageTabStore;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            LoadCurrent = ReactiveCommand.CreateFromTask(LoadCurrentHandler);
            LoadUnload = ReactiveCommand.CreateFromTask(LoadUnloadHandler);
            LoadAll = ReactiveCommand.CreateFromTask(LoadAllHandler);
            LoadVillage = ReactiveCommand.Create<AccountId, List<ListBoxItem>>(LoadVillageHandler);

            var villageObservable = this.WhenAnyValue(x => x.Villages.SelectedItem);
            villageObservable.BindTo(_selectedItemStore, vm => vm.Village);
            villageObservable.Subscribe(x =>
            {
                var tabType = VillageTabType.Normal;
                if (x is null) tabType = VillageTabType.NoVillage;
                _villageTabStore.SetTabType(tabType);
            });

            LoadVillage.Subscribe(villages => Villages.Load(villages));
        }

        public async Task VillageListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadVillage.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadVillage.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        private async Task LoadCurrentHandler()
        {
            await _mediator.Send(new LoadCurrentCommand(AccountId, Villages));
        }

        private async Task LoadUnloadHandler()
        {
            await _mediator.Send(new LoadUnloadCommand(AccountId));
        }

        private async Task LoadAllHandler()
        {
            await _mediator.Send(new LoadAllCommand(AccountId));
        }

        private List<ListBoxItem> LoadVillageHandler(AccountId accountId)
        {
            return _unitOfRepository.VillageRepository.GetItems(accountId);
        }
    }
}