using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.Tabs.Villages;

using ReactiveUI.Primitives;

namespace MainCore.UI.Stores;

[RegisterSingleton<VillageTabStore>]
public partial class VillageTabStore : ViewModelBase
{
    private readonly bool[] _tabVisibility = new bool[2];
    private readonly NoVillageViewModel _noVillageViewModel;
    private readonly InfoViewModel _infoViewModel;
    private readonly VillageSettingViewModel _villageSettingViewModel;
    private readonly BuildViewModel _buildViewModel;

    private VillageTabType _currentTabType;

    [Reactive]
    private bool _isNoVillageTabVisible = true;

    [Reactive]
    private bool _isNormalTabVisible;

    public VillageTabStore(
        NoVillageViewModel noVillageViewModel,
        InfoViewModel infoViewModel,
        VillageSettingViewModel villageSettingViewModel,
        BuildViewModel buildViewModel,
        SelectedItemStore selectedItemStore)
    {
        _noVillageViewModel = noVillageViewModel;
        _infoViewModel = infoViewModel;
        _villageSettingViewModel = villageSettingViewModel;
        _buildViewModel = buildViewModel;

        Init(selectedItemStore);
    }

    private void Init(SelectedItemStore selectedItemStore)
    {
        selectedItemStore.VillageChanges.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(villageId =>
        {
            var tabType = villageId == VillageId.Empty ? VillageTabType.NoVillage : VillageTabType.Normal;
            SetTabType(tabType);
        });
    }

    private void SetTabType(VillageTabType tabType)
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
            VillageTabType.NoVillage => _noVillageViewModel,
            VillageTabType.Normal => GetCurrentNormalTabViewModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null)
        };
    }

    private TabViewModelBase GetCurrentNormalTabViewModel()
    {
        if (_buildViewModel.IsActive) return _buildViewModel;
        if (_infoViewModel.IsActive) return _infoViewModel;
        if (_villageSettingViewModel.IsActive) return _villageSettingViewModel;

        return _buildViewModel;
    }

    public NoVillageViewModel NoVillageViewModel => _noVillageViewModel;
    public BuildViewModel BuildViewModel => _buildViewModel;
    public VillageSettingViewModel VillageSettingViewModel => _villageSettingViewModel;
    public InfoViewModel InfoViewModel => _infoViewModel;
}