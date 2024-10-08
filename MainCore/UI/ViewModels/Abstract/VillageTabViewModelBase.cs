using MainCore.UI.Stores;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract class VillageTabViewModelBase : TabViewModelBase
    {
        protected readonly SelectedItemStore _selectedItemStore;

        private readonly ObservableAsPropertyHelper<AccountId> _accountId;
        public AccountId AccountId => _accountId.Value;

        private readonly ObservableAsPropertyHelper<VillageId> _villageId;
        public VillageId VillageId => _villageId.Value;

        public ReactiveCommand<VillageId, Unit> VillageChanged { get; }

        protected VillageTabViewModelBase()
        {
            _selectedItemStore = Locator.Current.GetService<SelectedItemStore>();

            VillageChanged = ReactiveCommand.CreateFromTask<VillageId>(VillageChangedHandler);

            var accountIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Account)
                                            .WhereNotNull()
                                            .Select(x => new AccountId(x.Id));

            accountIdObservable.ToProperty(this, vm => vm.AccountId, out _accountId);

            var villageIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Village)
                                            .WhereNotNull()
                                            .Select(x => new VillageId(x.Id));

            villageIdObservable
                .ToProperty(this, vm => vm.VillageId, out _villageId);

            villageIdObservable
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(VillageChanged);
        }

        private async Task VillageChangedHandler(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId == VillageId.Empty) return;
            await Load(villageId);
        }

        protected override async Task OnActive()
        {
            if (VillageId == VillageId.Empty) return;
            await Load(VillageId);
        }

        protected abstract Task Load(VillageId villageId);
    }
}