using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.Tabs.Villages;

namespace MainCore.UI.Stores
{
    [RegisterSingleton<VillageTabStore>]
    public partial class VillageTabStore : ViewModelBase
    {
        private readonly bool[] _tabVisibility = new bool[2];
        private VillageTabType _currentTabType;

        [Reactive]
        private bool _isNoVillageTabVisible = true;

        [Reactive]
        private bool _isNormalTabVisible;

        private readonly NoVillageViewModel _noVillageViewModel;
        private readonly VillageSettingViewModel _villageSettingViewModel;
        private readonly AttackViewModel _attackViewModel;
        private readonly BuildViewModel _buildViewModel;
        private readonly InfoViewModel _infoViewModel;

        public VillageTabStore(NoVillageViewModel noVillageViewModel, InfoViewModel infoViewModel, BuildViewModel buildViewModel, VillageSettingViewModel villageSettingViewModel, AttackViewModel attackViewModel)
        {
            _noVillageViewModel = noVillageViewModel;
            _buildViewModel = buildViewModel;
            _infoViewModel = infoViewModel;
            _villageSettingViewModel = villageSettingViewModel;
            _attackViewModel = attackViewModel;
        }

        public void SetTabType(VillageTabType tabType)
        {
            if (tabType == _currentTabType) return;
            _currentTabType = tabType;

            for (int i = 0; i < _tabVisibility.Length; i++)
            {
                _tabVisibility[i] = false;
            }
            _tabVisibility[(int)tabType] = true;

            IsNoVillageTabVisible = _tabVisibility[(int)VillageTabType.NoVillage];
            IsNormalTabVisible = _tabVisibility[(int)VillageTabType.Normal];

            switch (tabType)
            {
                case VillageTabType.NoVillage:
                    _noVillageViewModel.IsActive = true;
                    break;

                case VillageTabType.Normal:
                    _buildViewModel.IsActive = true;
                    _attackViewModel.IsActive = true;
                    break;

                default:
                    break;
            }
        }

        public NoVillageViewModel NoVillageViewModel => _noVillageViewModel;
        public BuildViewModel BuildViewModel => _buildViewModel;
        public VillageSettingViewModel VillageSettingViewModel => _villageSettingViewModel;
        public InfoViewModel InfoViewModel => _infoViewModel;
        public AttackViewModel AttackViewModel => _attackViewModel;
    }
}