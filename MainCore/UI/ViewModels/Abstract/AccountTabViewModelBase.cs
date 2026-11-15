using MainCore.UI.Stores;

using ReactiveUI.Primitives;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract partial class AccountTabViewModelBase : TabViewModelBase
    {
        [ObservableAsProperty]
        private AccountId _accountId;

        protected SelectedItemStore _selectedItemStore;

        protected AccountTabViewModelBase(DialogService dialogService, SelectedItemStore selectedItemStore)
            : base(dialogService)
        {
            _selectedItemStore = selectedItemStore;

            _accountIdHelper = selectedItemStore.AccountChanges.ToProperty(this, vm => vm.AccountId);

            selectedItemStore.AccountChanges
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
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