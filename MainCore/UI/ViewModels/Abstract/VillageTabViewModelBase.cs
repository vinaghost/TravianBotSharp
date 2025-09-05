using MainCore.UI.Stores;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract partial class VillageTabViewModelBase : TabViewModelBase
    {
        protected readonly SelectedItemStore _selectedItemStore;

        [ObservableAsProperty]
        private AccountId _accountId;

        [ObservableAsProperty]
        private VillageId _villageId;

        protected VillageTabViewModelBase()
        {
            _selectedItemStore = Locator.Current.GetService<SelectedItemStore>()!;

            var accountIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Account)
                                            .WhereNotNull()
                                            .Select(x => new AccountId(x.Id));

            _accountIdHelper = accountIdObservable.ToProperty(this, vm => vm.AccountId);

            var villageIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Village)
                                            .WhereNotNull()
                                            .Select(x => new VillageId(x.Id));

            _villageIdHelper = villageIdObservable.ToProperty(this, vm => vm.VillageId);

            villageIdObservable
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(VillageChangedCommand);
        }

        [ReactiveCommand]
        private async Task VillageChanged(VillageId villageId)
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
