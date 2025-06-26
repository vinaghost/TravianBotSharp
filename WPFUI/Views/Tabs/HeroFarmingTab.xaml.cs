using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs
{
    public class HeroFarmingTabBase : ReactiveUserControl<HeroFarmingViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for HeroFarmingTab.xaml
    /// </summary>
    public partial class HeroFarmingTab : HeroFarmingTabBase
    {
        public HeroFarmingTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Oasises.Items, v => v.OasisesGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Oasises.SelectedItem, v => v.OasisesGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Oasises.SelectedIndex, v => v.OasisesGrid.SelectedIndex).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.StartCommand, v => v.StartButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StopCommand, v => v.StopButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveCommand, v => v.SaveButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.X, v => v.X.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Y, v => v.Y.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddCommand, v => v.AddButton).DisposeWith(d);

                //this.BindCommand(ViewModel, vm => vm.UpCommand, v => v.UpButton).DisposeWith(d);
                //this.BindCommand(ViewModel, vm => vm.DownCommand, v => v.DownButton).DisposeWith(d);
                //this.BindCommand(ViewModel, vm => vm.TopCommand, v => v.TopButton).DisposeWith(d);
                //this.BindCommand(ViewModel, vm => vm.BottomCommand, v => v.BottomButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DeleteCommand, v => v.DeleteButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DeleteAllCommand, v => v.DeleteAllButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.ImportCommand, v => v.ImportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ExportCommand, v => v.ExportButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeroFarmingHealthCondition, v => v.HeroFarmingHealthCondition.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountSettingInput.HeroFarmingIgnoreCondition, v => v.HeroFarmingIgnoreCondition.Text).DisposeWith(d);
            });
        }
    }
}