using MainCore.UI.Stores;

namespace MainCore.UI.ViewModels.Abstract
{
    using ReactiveUI.Primitives;

    using Splat;

    public abstract partial class AccountTabViewModelBase : TabViewModelBase
    {
        protected readonly SelectedItemStore _selectedItemStore;

        [ObservableAsProperty]
        private AccountId _accountId;

        protected AccountTabViewModelBase()
        {
            _selectedItemStore = Locator.Current.GetService<SelectedItemStore>()!;

            var accountIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Account)
                                        .Select(x => x is null ? AccountId.Empty : new AccountId(x.Id));

            _accountIdHelper = accountIdObservable.ToProperty(this, vm => vm.AccountId);

            accountIdObservable
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .InvokeCommand(AccountChangedCommand);
        }

        [ReactiveCommand]
        private async Task AccountChanged(AccountId accountId)
        {
            if (!IsActive) return;

            if (accountId == AccountId.Empty)
            {
                await OnAccountContextInvalidated();
                return;
            }

            await Load(accountId);
        }

        protected override async Task OnActive()
        {
            if (AccountId == AccountId.Empty)
            {
                await OnAccountContextInvalidated();
                return;
            }

            await Load(AccountId);
        }

        protected virtual Task OnAccountContextInvalidated()
        {
            return Task.CompletedTask;
        }

        protected abstract Task Load(AccountId accountId);
    }
}