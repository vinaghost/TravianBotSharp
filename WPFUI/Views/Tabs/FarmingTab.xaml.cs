using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs
{
    public class FarmingTabBase : ReactiveUserControl<FarmingViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for FarmingTab.xaml
    /// </summary>
    public partial class FarmingTab : FarmingTabBase
    {
        public FarmingTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Start, v => v.StartButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Stop, v => v.StopButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ActiveFarmList, v => v.ActiveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UpdateFarmList, v => v.Load).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.FarmLists.Items, v => v.FarmlistGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FarmLists.SelectedItem, v => v.FarmlistGrid.SelectedItem).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.FarmListSettingInput.UseStartAllButton, v => v.UseStartAllCheckbox.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.FarmListSettingInput.FarmInterval, v => v.FarmInterval.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FarmListSettingInput.EnableAutoDisableRedRaidReport, v => v.EnableAutoDisableRedRaidReport.IsChecked).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.ActiveText, v => v.ActiveButton.Content).DisposeWith(d);
            });
        }
    }
}