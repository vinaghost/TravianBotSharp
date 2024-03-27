using MainCore.UI.ViewModels.Tabs.Villages;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs.Villages
{
    public class BuildTabBase : ReactiveUserControl<BuildViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for BuildTab.xaml
    /// </summary>
    public partial class BuildTab : BuildTabBase
    {
        public BuildTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Buildings.Items, v => v.BuildingsGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Buildings.SelectedItem, v => v.BuildingsGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Buildings.SelectedIndex, v => v.BuildingsGrid.SelectedIndex).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Jobs.Items, v => v.JobsGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Jobs.SelectedItem, v => v.JobsGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Jobs.SelectedIndex, v => v.JobsGrid.SelectedIndex).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Queue.Items, v => v.QueueGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Jobs.SelectedItem, v => v.QueueGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Jobs.SelectedIndex, v => v.QueueGrid.SelectedIndex).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.UpgradeOneLevel, v => v.UpgradeOneLevelButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UpgradeMaxLevel, v => v.UpgradeMaxLevelButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.Import, v => v.ImportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Export, v => v.ExportButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.Up, v => v.UpButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Down, v => v.DownButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Top, v => v.TopButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Bottom, v => v.BottomButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Delete, v => v.DeleteButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DeleteAll, v => v.DeleteAllButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.BuildNormal, v => v.NormalBuild).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NormalBuildInput.Buildings, v => v.NormalBuildings.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NormalBuildInput.SelectedBuilding, v => v.NormalBuildings.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NormalBuildInput.Level, v => v.NormalLevel.Text).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.BuildResource, v => v.ResourceBuild).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ResourceBuildInput.Plans, v => v.ResType.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ResourceBuildInput.SelectedPlan, v => v.ResType.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ResourceBuildInput.Level, v => v.ResourceLevel.Text).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.TrainTroop, v => v.TrainTroop).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TrainTroopInput.Type, v => v.TrainTroopType.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.TrainTroopInput.Great, v => v.TrainTroopGreat.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.TrainTroopInput.Amount, v => v.TrainTroopAmount.Text).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.ResearchTroop, v => v.ResearchTroop).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ResearchTroopInput.Type, v => v.ResearchTroopType.ViewModel).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.Celebration, v => v.Celebration).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CelebrationInput.Great, v => v.CelebrationGreat.IsChecked).DisposeWith(d);
            });
        }
    }
}