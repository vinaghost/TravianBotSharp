using MainCore.UI.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.Tabs;

namespace MainCore.UI.Stores
{
    [RegisterSingleton<AccountTabStore>]
    public partial class AccountTabStore : ViewModelBase
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

        private readonly NoAccountViewModel _noAccountViewModel;
        private readonly AddAccountViewModel _addAccountViewModel;
        private readonly AddAccountsViewModel _addAccountsViewModel;
        private readonly AccountSettingViewModel _accountSettingViewModel;
        private readonly VillageViewModel _villageViewModel;
        private readonly EditAccountViewModel _editAccountViewModel;
        private readonly FarmingViewModel _farmingViewModel;
        private readonly DebugViewModel _debugViewModel;

        public AccountTabStore(NoAccountViewModel noAccountViewModel, AddAccountViewModel addAccountViewModel, AddAccountsViewModel addAccountsViewModel, EditAccountViewModel editAccountViewModel, DebugViewModel debugViewModel, AccountSettingViewModel accountSettingViewModel, VillageViewModel villageViewModel, FarmingViewModel farmingViewModel)
        {
            _noAccountViewModel = noAccountViewModel;
            _addAccountViewModel = addAccountViewModel;
            _addAccountsViewModel = addAccountsViewModel;
            _accountSettingViewModel = accountSettingViewModel;
            _editAccountViewModel = editAccountViewModel;
            _debugViewModel = debugViewModel;
            _villageViewModel = villageViewModel;
            _farmingViewModel = farmingViewModel;
        }

        public void SetTabType(AccountTabType tabType)
        {
            if (tabType == _currentTabType)
            {
                if (tabType == AccountTabType.NoAccount && !_noAccountViewModel.IsActive)
                {
                    _noAccountViewModel.IsActive = true;
                }
                return;
            }
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

            switch (tabType)
            {
                case AccountTabType.NoAccount:
                    _noAccountViewModel.IsActive = true;
                    break;

                case AccountTabType.Normal:
                    _accountSettingViewModel.IsActive = true;
                    break;

                case AccountTabType.AddAccount:
                    _addAccountViewModel.IsActive = true;
                    break;

                case AccountTabType.AddAccounts:
                    _addAccountsViewModel.IsActive = true;
                    break;

                default:
                    break;
            }
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
}