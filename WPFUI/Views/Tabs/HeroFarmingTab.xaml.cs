using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;

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
        }
    }
}