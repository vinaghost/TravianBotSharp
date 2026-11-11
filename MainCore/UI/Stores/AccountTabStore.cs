using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.UI.Stores
{
    [RegisterSingleton<AccountTabStore>]
    public partial class AccountTabStore(
        NoAccountViewModel noAccountViewModel,
        AddAccountViewModel addAccountViewModel,
        AddAccountsViewModel addAccountsViewModel,
        EditAccountViewModel editAccountViewModel,
        DebugViewModel debugViewModel,
        AccountSettingViewModel accountSettingViewModel,
        VillageViewModel villageViewModel,
        FarmingViewModel farmingViewModel) : ViewModelBase
    {
        private readonly bool[] _tabVisibility = new bool[4];
        private AccountTabType _currentTabType;

        [Reactive]
        private bool _isNoAccountTabVisible = true;

        [Reactive]
        private bool _isAddAccountTabVisible;

        [Reactive]
        private bool _isAddAccountsTabVisible;

        [Reactive]
        private bool _isNormalTabVisible;

        public void SetTabType(AccountTabType tabType)
        {
            if (tabType == _currentTabType)
            {
                GetTabViewModel(tabType).IsActive = true;
                return;
            }

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
                AccountTabType.NoAccount => noAccountViewModel,
                AccountTabType.AddAccount => addAccountViewModel,
                AccountTabType.AddAccounts => addAccountsViewModel,
                AccountTabType.Normal => GetCurrentNormalTabViewModel(),
                _ => throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null)
            };
        }

        private TabViewModelBase GetCurrentNormalTabViewModel()
        {
            if (villageViewModel.IsActive) return villageViewModel;
            if (editAccountViewModel.IsActive) return editAccountViewModel;
            if (farmingViewModel.IsActive) return farmingViewModel;
            if (debugViewModel.IsActive) return debugViewModel;

            return accountSettingViewModel;
        }

        public NoAccountViewModel NoAccountViewModel => noAccountViewModel;
        public AddAccountViewModel AddAccountViewModel => addAccountViewModel;
        public AddAccountsViewModel AddAccountsViewModel => addAccountsViewModel;
        public AccountSettingViewModel AccountSettingViewModel => accountSettingViewModel;
        public VillageViewModel VillageViewModel => villageViewModel;
        public EditAccountViewModel EditAccountViewModel => editAccountViewModel;
        public DebugViewModel DebugViewModel => debugViewModel;
        public FarmingViewModel FarmingViewModel => farmingViewModel;
    }
}