using MainCore.UI.Stores;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract partial class AccountTabViewModelBase : TabViewModelBase
    {
        protected readonly SelectedItemStore _selectedItemStore;

        [ObservableAsProperty]
        private AccountId _accountId;

        protected AccountTabViewModelBase()
        {
            _selectedItemStore = Locator.Current.GetService<SelectedItemStore>()!;

            var accountIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Account)
                                        .WhereNotNull()
                                        .Select(x => new AccountId(x.Id));

            _accountIdHelper = accountIdObservable.ToProperty(this, vm => vm.AccountId);

            accountIdObservable
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(AccountChangedCommand);
        }

        [ReactiveCommand]
        private async Task AccountChanged(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId == AccountId.Empty) return;
            await Load(accountId);
        }

        protected override async Task OnActive()
        {
            if (AccountId == AccountId.Empty) return;
            await Load(AccountId);
        }

        protected abstract Task Load(AccountId accountId);
    }
}
