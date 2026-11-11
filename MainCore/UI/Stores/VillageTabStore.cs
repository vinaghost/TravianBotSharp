using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.Tabs.Villages;
using MainCore.Shared.Enums;

namespace MainCore.UI.Stores
{
    [RegisterSingleton<VillageTabStore>]
    public partial class VillageTabStore(
        NoVillageViewModel noVillageViewModel,
        InfoViewModel infoViewModel,
        VillageSettingViewModel villageSettingViewModel,
        BuildViewModel buildViewModel) : ViewModelBase
    {
        private readonly bool[] _tabVisibility = new bool[2];
        private VillageTabType _currentTabType;

        [Reactive]
        private bool _isNoVillageTabVisible = true;

        [Reactive]
        private bool _isNormalTabVisible;

        public void SetTabType(VillageTabType tabType)
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

            IsNoVillageTabVisible = _tabVisibility[(int)VillageTabType.NoVillage];
            IsNormalTabVisible = _tabVisibility[(int)VillageTabType.Normal];

            GetTabViewModel(tabType).IsActive = true;
        }

        private TabViewModelBase GetTabViewModel(VillageTabType tabType)
        {
            return tabType switch
            {
                VillageTabType.NoVillage => noVillageViewModel,
                VillageTabType.Normal => GetCurrentNormalTabViewModel(),
                _ => throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null)
            };
        }

        private TabViewModelBase GetCurrentNormalTabViewModel()
        {
            if (buildViewModel.IsActive) return buildViewModel;
            if (infoViewModel.IsActive) return infoViewModel;
            if (villageSettingViewModel.IsActive) return villageSettingViewModel;

            return buildViewModel;
        }

        public NoVillageViewModel NoVillageViewModel => noVillageViewModel;
        public BuildViewModel BuildViewModel => buildViewModel;
        public VillageSettingViewModel VillageSettingViewModel => villageSettingViewModel;
        public InfoViewModel InfoViewModel => infoViewModel;
    }
}