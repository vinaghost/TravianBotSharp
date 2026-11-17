using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.Stores;

[RegisterSingleton<SelectedItemStore>]
public partial class SelectedItemStore : ViewModelBase
{
    private readonly Dictionary<AccountId, VillageId> _lastSelectedVillageByAccount = [];

    public VillageId GetLastSelectedVillage(AccountId accountId)
    {
        if (_lastSelectedVillageByAccount.TryGetValue(accountId, out var villageId))
        {
            return villageId;
        }
        return VillageId.Empty;
    }

    public SelectedItemStore()
    {
        AccountChanges = this.WhenAnyValue(x => x.AccountId);
        VillageChanges = this.WhenAnyValue(x => x.VillageId);
    }

    public IObservable<AccountId> AccountChanges { get; }
    public IObservable<VillageId> VillageChanges { get; }

    public void SetAccount(ListBoxItem? account)
    {
        _lastSelectedVillageByAccount[AccountId] = VillageId;
        if (account is null)
        {
            AccountId = AccountId.Empty;
            return;
        }

        if (account.Id != AccountId.Value)
        {
            AccountId = new AccountId(account.Id);
        }
    }

    public void SetVillage(ListBoxItem? village)
    {
        _lastSelectedVillageByAccount[AccountId] = VillageId;

        if (village is null)
        {
            VillageId = VillageId.Empty;
            return;
        }

        if (village.Id != VillageId.Value)
        {
            VillageId = new VillageId(village.Id);
        }
    }

    [Reactive]
    private AccountId _accountId;

    [Reactive]
    private VillageId _villageId;
}