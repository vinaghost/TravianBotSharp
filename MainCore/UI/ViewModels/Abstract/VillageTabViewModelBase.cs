using MainCore.UI.Stores;
using ReactiveUI;
using Splat;
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

        public VillageTabViewModelBase()
        {
            _selectedItemStore = Locator.Current.GetService<SelectedItemStore>();

            VillageChanged = ReactiveCommand.CreateFromTask<VillageId>(VillageChangedHandler);

            var accountIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Account)
                                       .Select(x => new AccountId(x?.Id ?? 0));

            accountIdObservable.ToProperty(this, vm => vm.AccountId, out _accountId);

            var villageIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Village)
                                        .Select(x => new VillageId(x?.Id ?? 0));

            villageIdObservable.ToProperty(this, vm => vm.VillageId, out _villageId);
            villageIdObservable.InvokeCommand(VillageChanged);
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