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
                this.OneWayBind(ViewModel, vm => vm.Oasises, v => v.OasisesGrid.ItemsSource).DisposeWith(d);
            });
        }
    }
}