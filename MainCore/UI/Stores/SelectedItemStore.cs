using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.Stores
{
    [RegisterSingleton<SelectedItemStore>]
    public partial class SelectedItemStore : ViewModelBase
    {
        public SelectedItemStore()
        {
            AccountChanges = this.WhenAnyValue(vm => vm.AccountId);
            VillageChanges = this.WhenAnyValue(vm => vm.VillageId);
        }

        public IObservable<AccountId> AccountChanges { get; }

        public IObservable<VillageId> VillageChanges { get; }

        public void SetAccount(ListBoxItem? account)
        {
            if (account is null)
            {
                if (AccountId != AccountId.Empty)
                {
                    AccountId = AccountId.Empty;
                    ClearVillage();
                }

                return;
            }

            if (account.Id != AccountId.Value)
            {
                AccountId = new AccountId(account.Id);
                ClearVillage();
            }
        }

        public void SetVillage(ListBoxItem? village)
        {
            if (village is null)
            {
                if (VillageId != VillageId.Empty)
                {
                    VillageId = VillageId.Empty;
                }

                return;
            }

            if (village.Id != VillageId.Value)
            {
                VillageId = new VillageId(village.Id);
            }
        }

        public void ClearVillage()
        {
            VillageId = VillageId.Empty;
        }

        [Reactive]
        private AccountId _accountId;

        [Reactive]
        private VillageId _villageId;
    }
}