using MainCore.UI.Stores;

using ReactiveUI.Primitives;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract partial class VillageTabViewModelBase : TabViewModelBase
    {
        protected readonly SelectedItemStore _selectedItemStore;

        [ObservableAsProperty]
        private AccountId _accountId;

        [ObservableAsProperty]
        private VillageId _villageId;

        protected VillageTabViewModelBase(DialogService dialogService, SelectedItemStore selectedItemStore) : base(dialogService)
        {
            _selectedItemStore = selectedItemStore;

            _accountIdHelper = _selectedItemStore.AccountChanges.ToProperty(this, vm => vm.AccountId);
            _villageIdHelper = _selectedItemStore.VillageChanges.ToProperty(this, vm => vm.VillageId);

            _selectedItemStore.VillageChanges
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
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
            await Load(VillageId);
        }

        protected abstract Task Load(VillageId villageId);
    }
}