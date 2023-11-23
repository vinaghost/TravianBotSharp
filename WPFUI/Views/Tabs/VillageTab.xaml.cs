using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs
{
    public class VillageTabBase : ReactiveUserControl<VillageViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for VillageTab.xaml
    /// </summary>
    public partial class VillageTab : VillageTabBase
    {
        public VillageTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.LoadCurrentCommand, v => v.LoadCurrent).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LoadUnloadCommand, v => v.LoadUnload).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LoadAllCommand, v => v.LoadAll).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Villages.Items, v => v.VillagesGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Villages.SelectedItem, v => v.VillagesGrid.SelectedItem).DisposeWith(d);

                // tabs
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.NoVillageViewModel, v => v.NoVillage.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.BuildViewModel, v => v.Build.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.InfoViewModel, v => v.Info.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.VillageSettingViewModel, v => v.Settings.ViewModel).DisposeWith(d);

                // visible
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.IsNoVillageTabVisible, v => v.NoVillageTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.IsNormalTabVisible, v => v.BuildTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.IsNormalTabVisible, v => v.SettingsTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.VillageTabStore.IsNormalTabVisible, v => v.InfoTab.Visibility).DisposeWith(d);

                // selected
                this.Bind(ViewModel, vm => vm.VillageTabStore.NoVillageViewModel.IsActive, v => v.NoVillageTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageTabStore.BuildViewModel.IsActive, v => v.BuildTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageTabStore.VillageSettingViewModel.IsActive, v => v.SettingsTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.VillageTabStore.InfoViewModel.IsActive, v => v.InfoTab.IsSelected).DisposeWith(d);
            });
        }
    }
}