using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.Tabs;

using ReactiveUI.Primitives;

namespace MainCore.UI.Stores;

public enum AccountTabType
{
    NoAccount = 0,
    AddAccount,
    AddAccounts,
    Normal,
}

public enum AccountNormalTabType
{
    AccountSetting = 0,
    Village,
    EditAccount,
    Farming,
    Debug,
}

[RegisterSingleton<AccountTabStore>]
public partial class AccountTabStore : ViewModelBase
{
    private readonly bool[] _tabVisibility = new bool[4];
    private readonly NoAccountViewModel _noAccountViewModel;
    private readonly AddAccountViewModel _addAccountViewModel;
    private readonly AddAccountsViewModel _addAccountsViewModel;
    private readonly EditAccountViewModel _editAccountViewModel;
    private readonly DebugViewModel _debugViewModel;
    private readonly AccountSettingViewModel _accountSettingViewModel;
    private readonly VillageViewModel _villageViewModel;
    private readonly FarmingViewModel _farmingViewModel;

    private readonly VillageTabStore _villageTabStore;

    private AccountTabType _currentTabType;

    private AccountNormalTabType _currentNormalTabType;

    [Reactive]
    private bool _isNoAccountTabVisible = true;

    [Reactive]
    private bool _isAddAccountTabVisible;

    [Reactive]
    private bool _isAddAccountsTabVisible;

    [Reactive]
    private bool _isNormalTabVisible;

    public AccountTabStore(
        NoAccountViewModel noAccountViewModel,
        AddAccountViewModel addAccountViewModel,
        AddAccountsViewModel addAccountsViewModel,
        EditAccountViewModel editAccountViewModel,
        DebugViewModel debugViewModel,
        AccountSettingViewModel accountSettingViewModel,
        VillageViewModel villageViewModel,
        FarmingViewModel farmingViewModel,
        SelectedItemStore selectedItemStore,
        VillageTabStore villageTabStore)
    {
        _noAccountViewModel = noAccountViewModel;
        _addAccountViewModel = addAccountViewModel;
        _addAccountsViewModel = addAccountsViewModel;
        _editAccountViewModel = editAccountViewModel;
        _debugViewModel = debugViewModel;
        _accountSettingViewModel = accountSettingViewModel;
        _villageViewModel = villageViewModel;
        _farmingViewModel = farmingViewModel;
        _villageTabStore = villageTabStore;

        Init(selectedItemStore);
    }

    private void Init(SelectedItemStore selectedItemStore)
    {
        selectedItemStore.AccountChanges.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(accountId =>
        {
            if (accountId == AccountId.Empty)
            {
                if (_currentTabType == AccountTabType.AddAccount || _currentTabType == AccountTabType.AddAccounts) return;
                Active(AccountTabType.NoAccount);
                return;
            }

            Active(AccountTabType.Normal);
        });

        _accountSettingViewModel.WhenAnyValue(x => x.IsActive)
            .Where(isActive => isActive)
            .Subscribe(_ => _currentNormalTabType = AccountNormalTabType.AccountSetting);

        _villageViewModel.WhenAnyValue(x => x.IsActive)
            .Where(isActive => isActive)
            .Subscribe(_ =>
            {
                _currentNormalTabType = AccountNormalTabType.Village;
                var villageId = selectedItemStore.VillageId;
                var tabType = villageId == VillageId.Empty ? VillageTabType.NoVillage : VillageTabType.Normal;
                _villageTabStore.Active(tabType);
            });

        _villageViewModel.WhenAnyValue(x => x.IsActive)
            .Where(isActive => !isActive)
            .Subscribe(_ => _villageTabStore.Deactive());

        _editAccountViewModel.WhenAnyValue(x => x.IsActive)
            .Where(isActive => isActive)
            .Subscribe(_ => _currentNormalTabType = AccountNormalTabType.EditAccount);

        _farmingViewModel.WhenAnyValue(x => x.IsActive)
            .Where(isActive => isActive)
            .Subscribe(_ => _currentNormalTabType = AccountNormalTabType.Farming);

        _debugViewModel.WhenAnyValue(x => x.IsActive)
            .Where(isActive => isActive)
            .Subscribe(_ => _currentNormalTabType = AccountNormalTabType.Debug);
    }

    public void Active(AccountTabType tabType)
    {
        if (tabType == _currentTabType) return;

        GetTabViewModel(_currentTabType).IsActive = false;
        _currentTabType = tabType;

        for (int i = 0; i < _tabVisibility.Length; i++)
        {
            _tabVisibility[i] = false;
        }
        _tabVisibility[(int)tabType] = true;

        IsNoAccountTabVisible = _tabVisibility[(int)AccountTabType.NoAccount];
        IsAddAccountTabVisible = _tabVisibility[(int)AccountTabType.AddAccount];
        IsAddAccountsTabVisible = _tabVisibility[(int)AccountTabType.AddAccounts];
        IsNormalTabVisible = _tabVisibility[(int)AccountTabType.Normal];

        GetTabViewModel(tabType).IsActive = true;
    }

    private TabViewModelBase GetTabViewModel(AccountTabType tabType)
    {
        return tabType switch
        {
            AccountTabType.NoAccount => _noAccountViewModel,
            AccountTabType.AddAccount => _addAccountViewModel,
            AccountTabType.AddAccounts => _addAccountsViewModel,
            AccountTabType.Normal => GetCurrentNormalTabViewModel(_currentNormalTabType),
            _ => throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null)
        };
    }

    private TabViewModelBase GetCurrentNormalTabViewModel(AccountNormalTabType normalTabType)
    {
        return normalTabType switch
        {
            AccountNormalTabType.AccountSetting => _accountSettingViewModel,
            AccountNormalTabType.Village => _villageViewModel,
            AccountNormalTabType.EditAccount => _editAccountViewModel,
            AccountNormalTabType.Farming => _farmingViewModel,
            AccountNormalTabType.Debug => _debugViewModel,
            _ => throw new ArgumentOutOfRangeException(nameof(normalTabType), normalTabType, null)
        };
    }

    public NoAccountViewModel NoAccountViewModel => _noAccountViewModel;
    public AddAccountViewModel AddAccountViewModel => _addAccountViewModel;
    public AddAccountsViewModel AddAccountsViewModel => _addAccountsViewModel;
    public AccountSettingViewModel AccountSettingViewModel => _accountSettingViewModel;
    public VillageViewModel VillageViewModel => _villageViewModel;
    public EditAccountViewModel EditAccountViewModel => _editAccountViewModel;
    public DebugViewModel DebugViewModel => _debugViewModel;
    public FarmingViewModel FarmingViewModel => _farmingViewModel;
}