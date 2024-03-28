using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs
{
    public class SettleTabBase : ReactiveUserControl<SettleViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for SettleTab.xaml
    /// </summary>
    public partial class SettleTab : SettleTabBase
    {
        public SettleTab()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.Add, v => v.AddButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Delete, v => v.DeleteButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AccountImport, v => v.ImportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.VillageImport, v => v.VillageImportButton).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.X, v => v.X.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Y, v => v.Y.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Path, v => v.Path.Text).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.NewVillages.Items, v => v.NewVillages.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NewVillages.SelectedItem, v => v.NewVillages.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NewVillages.SelectedIndex, v => v.NewVillages.SelectedIndex).DisposeWith(d);
            });
        }
    }
}