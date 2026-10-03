using MainCore.UI.Stores;

namespace MainCore.UI.ViewModels.Abstract
{
    using ReactiveUI.Primitives;

    public readonly record struct VillageContext(AccountId AccountId, VillageId VillageId, bool IsValid);

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
                                            .Select(x => x is null ? AccountId.Empty : new AccountId(x.Id));

            _accountIdHelper = accountIdObservable.ToProperty(this, vm => vm.AccountId);

            var villageIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Village)
                                            .Select(x => x is null ? VillageId.Empty : new VillageId(x.Id));

            _villageIdHelper = villageIdObservable.ToProperty(this, vm => vm.VillageId);

            accountIdObservable
                .CombineLatest(villageIdObservable, (accountId, villageId) => new VillageContext(accountId, villageId, accountId != AccountId.Empty && villageId != VillageId.Empty))
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .Subscribe(context => _ = VillageContextChanged(context));
        }

        private async Task VillageContextChanged(VillageContext context)
        {
            if (!IsActive) return;

            if (!context.IsValid)
            {
                await OnContextInvalidated(context.AccountId);
                return;
            }

            await Load(context.AccountId, context.VillageId);
        }

        protected override async Task OnActive()
        {
            if (AccountId == AccountId.Empty || VillageId == VillageId.Empty)
            {
                await OnContextInvalidated(AccountId);
                return;
            }

            await Load(AccountId, VillageId);
        }

        protected virtual Task OnContextInvalidated(AccountId accountId)
        {
            return Task.CompletedTask;
        }

        protected abstract Task Load(AccountId accountId, VillageId villageId);
    }
}