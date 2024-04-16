using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.UI.Enums;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;

namespace MainCore.UI.Stores
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class AccountTabStore : ViewModelBase
    {
        private readonly bool[] _tabVisibility = new bool[4];
        private AccountTabType _currentTabType;

        private bool _isNoAccountTabVisible = true;
        private bool _isAddAccountTabVisible;
        private bool _isAddAccountsTabVisible;
        private bool _isNormalTabVisible;

        private readonly NoAccountViewModel _noAccountViewModel;
        private readonly AddAccountViewModel _addAccountViewModel;
        private readonly AddAccountsViewModel _addAccountsViewModel;
        private readonly AccountSettingViewModel _accountSettingViewModel;
        private readonly HeroViewModel _heroViewModel;
        private readonly VillageViewModel _villageViewModel;
        private readonly EditAccountViewModel _editAccountViewModel;
        private readonly FarmingViewModel _farmingViewModel;
        private readonly SettleViewModel _settleViewModel;
        private readonly AlertViewModel _alertViewModel;
        private readonly DebugViewModel _debugViewModel;

        public AccountTabStore(NoAccountViewModel noAccountViewModel, AddAccountViewModel addAccountViewModel, AddAccountsViewModel addAccountsViewModel, EditAccountViewModel editAccountViewModel, DebugViewModel debugViewModel, AccountSettingViewModel accountSettingViewModel, VillageViewModel villageViewModel, FarmingViewModel farmingViewModel, HeroViewModel heroViewModel, SettleViewModel settleViewModel, AlertViewModel alertViewModel)
        {
            _noAccountViewModel = noAccountViewModel;
            _addAccountViewModel = addAccountViewModel;
            _addAccountsViewModel = addAccountsViewModel;
            _accountSettingViewModel = accountSettingViewModel;
            _editAccountViewModel = editAccountViewModel;
            _debugViewModel = debugViewModel;
            _villageViewModel = villageViewModel;
            _farmingViewModel = farmingViewModel;
            _heroViewModel = heroViewModel;
            _settleViewModel = settleViewModel;
            _alertViewModel = alertViewModel;
        }

        public void SetTabType(AccountTabType tabType)
        {
            if (tabType == _currentTabType)
            {
                if (tabType == AccountTabType.NoAccount)
                {
                    if (!_noAccountViewModel.IsActive) _noAccountViewModel.IsActive = true;
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

        public bool IsNoAccountTabVisible
        {
            get => _isNoAccountTabVisible;
            set => this.RaiseAndSetIfChanged(ref _isNoAccountTabVisible, value);
        }

        public bool IsAddAccountTabVisible
        {
            get => _isAddAccountTabVisible;
            set => this.RaiseAndSetIfChanged(ref _isAddAccountTabVisible, value);
        }

        public bool IsAddAccountsTabVisible
        {
            get => _isAddAccountsTabVisible;
            set => this.RaiseAndSetIfChanged(ref _isAddAccountsTabVisible, value);
        }

        public bool IsNormalTabVisible
        {
            get => _isNormalTabVisible;
            set => this.RaiseAndSetIfChanged(ref _isNormalTabVisible, value);
        }

        public NoAccountViewModel NoAccountViewModel => _noAccountViewModel;
        public AddAccountViewModel AddAccountViewModel => _addAccountViewModel;
        public AddAccountsViewModel AddAccountsViewModel => _addAccountsViewModel;
        public AccountSettingViewModel AccountSettingViewModel => _accountSettingViewModel;
        public VillageViewModel VillageViewModel => _villageViewModel;
        public EditAccountViewModel EditAccountViewModel => _editAccountViewModel;
        public DebugViewModel DebugViewModel => _debugViewModel;
        public FarmingViewModel FarmingViewModel => _farmingViewModel;
        public HeroViewModel HeroViewModel => _heroViewModel;
        public SettleViewModel SettleViewModel => _settleViewModel;
        public AlertViewModel AlertViewModel => _alertViewModel;
    }
}