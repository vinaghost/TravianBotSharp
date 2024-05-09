using MainCore.UI.Stores;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Abstract
{
    public abstract class AccountTabViewModelBase : TabViewModelBase
    {
        protected readonly SelectedItemStore _selectedItemStore;

        private readonly ObservableAsPropertyHelper<AccountId> _accountId;
        public AccountId AccountId => _accountId.Value;

        private ReactiveCommand<AccountId, Unit> AccountChanged { get; }

        protected AccountTabViewModelBase()
        {
            _selectedItemStore = Locator.Current.GetService<SelectedItemStore>();

            AccountChanged = ReactiveCommand.CreateFromTask<AccountId>(AccountChangedHandler);

            var accountIdObservable = this.WhenAnyValue(vm => vm._selectedItemStore.Account)
                                        .Select(x => new AccountId(x?.Id ?? 0));

            accountIdObservable.ToProperty(this, vm => vm.AccountId, out _accountId);
            accountIdObservable.InvokeCommand(AccountChanged);
        }

        private async Task AccountChangedHandler(AccountId accountId)
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